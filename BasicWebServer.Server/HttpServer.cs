using BasicWebServer.Server.Common;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BasicWebServer.Server
{
    public class HttpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly TcpListener serverListener;

        private readonly RoutingTable routingTable;

        // TOВА Е НАШИЯ inversion Of Control Kонтейнер (от BasicWebServer.Server/Common/ServiceCollection)
        // това го има и в класа Request (като пропърти).
        public readonly IServiceCollection ServiceCollection;
        // ще добавяме някакви сървисчета в startup-a (ще ги чейнваме при създаването на new HttpServer). т.е. на същото
        // място, където настройвахме съответните рутове (най в началото) ще настройваме и serviceCollection
        // в края на крайщата този serviceCollection трабва да стигне до Request класа (където трабва да го имаме)
        // виж в метода Parse на Request класа (там е подаден)

        public HttpServer (string ipAddress, int port, Action<IRoutingTable> routingTableConfiguration)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;

            this.serverListener = new TcpListener(this.ipAddress, this.port);

            routingTableConfiguration(this.routingTable = new RoutingTable());
            ServiceCollection = new ServiceCollection();
        }

        public HttpServer(int port, Action<IRoutingTable> routingTable)
            : this("127.0.0.1", port, routingTable)
        {
        }

        public HttpServer(Action<IRoutingTable> routingTable)
            : this(8080, routingTable)
        {

        }

        public async Task Start()
        {
            this.serverListener.Start();

            Console.WriteLine($"Server started on port {port}.");
            Console.WriteLine("Listening for requests...");

            // Ето какво прави цикъла по-долу:
            // 1.Създава конекция: var connection = await serverListener.AcceptTcpClientAsync();
            // 2.Изчаква да бъде създадена
            // 3.След което в отделна нишка се засилва всичко
            // 4.Конекцията остава жива до момента в който се получи connection.Close();
            // Всяко минаване през цикъла вдига нови конекции. Едни се откачат, други се закачат.
            // Така позволяваме да се закачат много потребители на този сървър. т.е. Най-общо казано –
            // стартираме в отделна нишка чакането за рекуеста и изкарването на респонса.
            // Така отделяме самия сървър и позволяваме едновременно да се слушат много заявки.

            // Записа е странен, но е много популярен, когато ще се викат делегати само на едно единствено място.
            // Ние можем да си декларираме колкото си искаме методи, но какъв е смисъла ако ще ги ползваме само веднъж.
            // Затова метода може да бъде подаден директно като параметър. Този делегат трябва да е асинхронен,
            // защото вътре имаме асинхронни операции… има await-и, които трябва да се ползват.

            // "_ =" означава, че най-накрая ще дискарднем резултата. В C# 10 в въведен дискард-а с цел по-лесна четимост
            // на кода (уж).  Дори да махнем "_ =" -> кода пак ще работи (VS, обаче ще го подчертвае в зелено, което дразни),
            // но трябва да сложим един await, за да може да се изчака резултата от нашата заявка. Идеята е, че щом се връща
            // Task, то този Task трябва някак си да го използваме. Ако няма да ни е нужно да го ползваме за понататъка
            // (т.е. да достъпим резултата от операцията (в случая имаме връщане на резултат т.е. операцията не е void)).
            // В случея VS ни предупреждава, че тази задача ще бъде пусната за изпълнение и не е ясно какво става с нея
            // (ние няма да разберем какво става с нея), но нас не ни интересува , а VS незнае, че нас не ни интересува.
            // Потребителите, които четат нашия код също незнаят, че нас не ни интересува. В случая – ние показваме,
            // че не ни интересува резултата. Дискарда няма нужда да се декларира. НО, докато този Task неприключи –
            // той няма да бъде унищожен от гарбидж колейтора т.е. ще се изчака той да приключи.

            // Task.Run –  това ни позволява да изпълним част от нашия код асинхронно
            // (по преценка на CLR-a – дали в същия тред или в отделен тред)

            // =>  защо се ползва ламбда? –> Таск.Рън очаква делегат, който да изпълни.
            // Т.е. трябва да рънне някакъв метод.

            while (true)
            {
                var connection = await serverListener.AcceptTcpClientAsync();

                _ = Task.Run(async () =>
                {
                    var networkStream = connection.GetStream();

                    // ReadRequest е асинхронен (виж по-долу. Mоже би беше добре в името на метода да има "Async". За яснота)
                    // прочитаме стрийма и си вземаме рекуеста
                    var requestText = await this.ReadRequest(networkStream);

                    Console.WriteLine(requestText);

                    // парсваме си рикуеста + подаваме и serviceCollection
                    var request = Request.Parse(requestText, ServiceCollection);

                    // мап-ваме ресонса с routing table-a 
                    var response = this.routingTable.MatchRequest(request);
                                      
                    // the session should be part of each response to the browser, затова:
                    AddSession(request, response);

                    // този метод е асинхронен (виж по-долу. Mоже би беше добре в името на метода да има "Async". За яснота)
                    await WriteResponse(networkStream, response);

                    connection.Close();
                });
            }
        }

        private void AddSession(Request request, Response response)
        {
            bool sessionExists = request.Session.ContainsKey(Session.SessionCurrentDateKey);

            if (!sessionExists)
            {
                // if the session doesn't not exist, create one with the current date and add it to the request to save it.
                request.Session[Session.SessionCurrentDateKey] = DateTime.Now.ToString();
                
                // Also, add a cookie to the response with the session cookie name as name and the session ID as value:
                response.Cookies.Add(Session.SessionCookieName, request.Session.Id);
            }
        }

        private async Task<string> ReadRequest(NetworkStream networkStream)
        {
            var bufferLength = 1024;
            var buffer = new byte[bufferLength];

            var totalBytes = 0;

            var requestBuilder = new StringBuilder();

            do
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferLength);

                totalBytes += bytesRead;

                if (totalBytes > 10 * 1024)
                {
                    throw new InvalidOperationException("Request is too large.");
                }

                requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            while (networkStream.DataAvailable); //May not run correctly over the Internet

            return requestBuilder.ToString();
        }

        private async Task WriteResponse(NetworkStream networkStream, Response response)
        {
            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());

            await networkStream.WriteAsync(responseBytes);
        }
    }
}