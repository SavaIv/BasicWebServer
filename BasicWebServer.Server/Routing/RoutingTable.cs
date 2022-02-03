using BasicWebServer.Server.Common;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;

namespace BasicWebServer.Server.Routing
{
    public class RoutingTable : IRoutingTable
    {
        // при инстанциранрто на HttpServer се извършва "напълването" на това Дикшинъри, където се мапва Рекуеста с
        // метода, който следва да го обработи
        private readonly Dictionary<Method, Dictionary<string, Func<Request, Response>>> routes;

        // този запис е т.н. стар синтаксис:
        //public RoutingTable() =>
        //    this.routes = new Dictionary<Method, Dictionary<string, Response>>()
        //    {
        //        [Method.Get] = new Dictionary<string, Response>(),
        //        [Method.Post] = new Dictionary<string, Response>(),
        //        [Method.Put] = new Dictionary<string, Response>(),
        //        [Method.Delete] = new Dictionary<string, Response>()
        //    };

        // имаме и нов синтаксис
        // ако компилатора може да се сети сам т.е. еднозначно е -> може да се изпише само new()
        // става дума за The Roslyn .NET compiler
        public RoutingTable() => this.routes = new()
        {
            // StringComparer.InvariantCultureIgnoreCase  <-- игнорира главните букви. Можеше да се изпише само така:
            // [Method.Get] = new()
            [Method.Get] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Post] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Put] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Delete] = new(StringComparer.InvariantCultureIgnoreCase)
        };

        // Обърни внимание, че метода Map връща IRoutingTable (т.е. RoutingTable) -> това дава възможност да извършим чейнването
        // при инстанцирането на HttpServer-a (виж Startup.cs, където е чейнванато: MapGet(bal).Mapget(blabla).Mapget(итн))
        // В този случай имаме следното: МаpGet -> връща Map -> Map-a връща текущата инстанция на RoutingTable
        // ЧЕЙНВАНЕ Е ВЪЗМОЖНО, КОГАТО МЕТОД(ИТЕ) ВРЪЩАТ СЪЩИЯ ОБЕКТ, ОТ КОЙТО СА БИЛИ ИЗВИКАН(И) - 
        // ТАКА МОЖЕ, ОТНОВО ДА СЕ ИЗВИКА МЕТОД ОТ ТОЗИ ОБЕКТ (метода Map от обекта RoutingTable, връща this (RoutingTable))
        public IRoutingTable Map(Method method, string path, Func<Request, Response> responseFunction)
        {
            // Func<Request, Response> - това е делагат, който трябва да получи параметър от тип request и да връща отговор от
            // тип response. 

            Guard.AgainstNull(path, nameof(path));
            Guard.AgainstNull(responseFunction, nameof(responseFunction));

            routes[method][path] = responseFunction;

            // return the current IRoutingTable instance т.е. връща целия клас (RoutingTable)
            return this;
        }

        public IRoutingTable MapGet(string path, Func<Request, Response> responseFunction)
            => Map(Method.Get, path, responseFunction);


        public IRoutingTable MapPost(string path, Func<Request, Response> responseFunction)
            => Map(Method.Post, path, responseFunction);


        // това се изпълнява в while цикъла на HttpServer-a (ред 95) -> където, този метод връща респонса
        // т.е. на практика това е най-важния метод. -> това е мачването на рекуеста към респонса (функцията, която ще върне
        // респонс)
        public Response MatchRequest(Request request)
        {
            var requestMethod = request.Method;
            var requestUrl = request.Url;

            if (!this.routes.ContainsKey(requestMethod)
                || !this.routes[requestMethod].ContainsKey(requestUrl))
            {
                return new NotFoundResponse();
            }

            // трябва да върнем Response, но този респонс, трябва да бъде намерен по някъкъв начин. Какво правим в този случай?
            //  - имаме делгат responseFunction в който записваме съответния раут --> ще трябва да си извадим този делегат
            var responseFunction = routes[requestMethod][requestUrl];
            // изкарваме (вземаме) сме си функцията - var-a e делегат, който е Func от нещоСи
            // това, което се иска обаче е да върнем респонс. Правим го така:          
            return responseFunction(request);
            // записа на делегата (Func<Request, Response>) е такъв -> получва като параметър Request и връща като 
            // резултат респонс
            // Func<T, V> imeNaDelegata -> T e типа на променливите, а V е типа на изхода (резултата)
            // след името на делагат, когато се подадат кръгли скоби (и се подаде входа) - то, делегата се изпълнява
            // t.e. ако напишем imeNaDelegata(Т)  <-- делгата следва да се изпълни
        }
    }
}
