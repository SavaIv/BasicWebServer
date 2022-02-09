using BasicWebServer.Server.Common;
using System.Linq;
using System.Web;

namespace BasicWebServer.Server.HTTP
{
    public class Request
    {
        // колекиция от сесиите на юзърите, които в момента са конектнати на сървъра
        private static Dictionary<string, Session> Sessions = new ();

        public Method Method { get; private set; }

        public string Url { get; private set; }

        public HeaderCollection Headers { get; private set; }

        public CookieCollection Cookies { get; set; }

        public string Body { get; private set; }

        public Session Session { get; private set; }

        // this is a field for the form data, which should be a dictionary holding key-value pairs
        // for the name and value of each form field
        public IReadOnlyDictionary<string, string> Form { get; private set; }
        public IReadOnlyDictionary<string, string> Query { get; private set; }

        // TOВА Е НАШИЯ inversion Of Control Kонтейнер (от BasicWebServer.Server/Common/ServiceCollection)
        // по принцип този ServiceCollection трябва да дойде от някъде и ние някак си да го настроим -> за тази цел, този
        // същия ServiceCollection ще го добавим на още едно място - в нашия HttpServer
        // static e за да може във всеки рекуест да е едно и също!
        public static IServiceCollection ServiceCollection { get; private set; }

        public static Request Parse(string request, IServiceCollection serviceCollection)
        {
            // ето така в рекуеста имаме сървис колекшън
            ServiceCollection = serviceCollection;

            var lines = request.Split("\r\n");

            var startLine = lines.First().Split(" ");

            var method = ParseMethod(startLine[0]);

            // във връзка с нуждата да вземем query стринга от url-то правим промяна в този ред
            // var url = startLine[1];  <-- който вече ще изглежда така: ще ни бъдат върнати две неща (това е Tuple)
            (string url, Dictionary<string, string> query) = ParseUrl(startLine[1]);

            var headers = ParseHeaders(lines.Skip(1));

            // ще подадем headers, които са прясно парснати на горния ред (кукитата се съдържат в хедърите)
            var cookies = ParseCookies(headers);

            // трябва ни сесията
            var session = GetSession(cookies);
            
            var bodyLines = lines.Skip(headers.Count + 2).ToArray();

            var body = string.Join("\r\n", bodyLines);

            var form = ParseForm(headers, body);

            return new Request
            {
                Method = method,
                Url = url,
                Headers = headers,
                Cookies = cookies,
                Body = body,
                Session = session,
                Form = form,
                Query = query
            };
        }

        private static (string url, Dictionary<string, string> query) ParseUrl(string queryString)
        {
            string url = string.Empty;
            Dictionary<string, string> query = new Dictionary<string, string>();

            // целта е да сплитнем по "?" за да може да вземем query string-a
            var parts = queryString.Split("?", 2); // констрентваме го до 2 (ако има още въпросителни => някой се бъзика)

            if (parts.Length > 1)  // т.е. сплитнало се е нещо 
            {
                var queryParams = parts[1].Split("&");

                foreach (var pair in queryParams)
                {
                    var param = pair.Split('=');
                    if (param.Length == 2)
                    {
                        query.Add(param[0], param[1]); 

                    }
                }
            }

            url = parts[0];           

            return (url, query);
        }

        private static Session GetSession(CookieCollection cookies)
        {
            // проверчване дали в куки колекцията имаме такива сесийно ИД
            var sessionId = cookies.Contains(Session.SessionCookieName)
                ? cookies[Session.SessionCookieName]
                : Guid.NewGuid().ToString();

            // проверяваме в колекцията със сесийни ИД-та дали имаме това ИД
            if (!Sessions.ContainsKey(sessionId))
            {
                Sessions[sessionId] = new Session(sessionId);
            }

            return Sessions[sessionId];
        }

        private static CookieCollection ParseCookies(HeaderCollection headers)
        {
            var cookieCollection = new CookieCollection();

            if (headers.Contains(Header.Cookie))
            {
                var cookieHeader = headers[Header.Cookie];
                var allCookies = cookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var cookieText in allCookies)
                {
                    var cookieParts = cookieText.Split('=', StringSplitOptions.RemoveEmptyEntries);

                    var cookieName = cookieParts[0].Trim();
                    var cookieValue = cookieParts[1].Trim();

                    cookieCollection.Add(cookieName, cookieValue);
                }
            }

            return cookieCollection;
        }

        // метода следва да върне collection of form data pairs
        private static Dictionary<string, string> ParseForm(HeaderCollection headers, string body)
        {
            var formCollection = new Dictionary<string, string>();

            if (headers.Contains(Header.ContentType)
                && headers[Header.ContentType] == ContentType.FormUrlEncoded)
            {
                var parsedResult = ParseFormData(body);

                foreach (var (name, value) in parsedResult)
                {
                    formCollection.Add(name, value);
                }
            }

            return formCollection;
        }

        // The method will accept the request body as a string, decode it and split it into parts
        // to get the key and value of each pair of form data
        private static Dictionary<string, string> ParseFormData(string bodyLines)
        => HttpUtility.UrlDecode(bodyLines)
            .Split("&")
            .Select(part => part.Split('='))
            .Where(part => part.Length == 2)
            .ToDictionary(
                part => part[0],
                part => part[1],
                StringComparer.InvariantCultureIgnoreCase);

        private static HeaderCollection ParseHeaders(IEnumerable<string> headerLines)
        {
            var headerCollection = new HeaderCollection();

            foreach (var headerLine in headerLines)
            {
                if (headerLine == string.Empty)
                {
                    break;
                }

                var headerParts = headerLine.Split(":", 2);

                if (headerParts.Length != 2)
                {
                    throw new InvalidOperationException("Request is not valid.");
                }

                var headerName = headerParts[0].Trim();
                var headerValue = headerParts[1].Trim();

                headerCollection.Add(headerName, headerValue);
            }

            return headerCollection;
        }

        private static Method ParseMethod(string method)
        {
            try
            {
                return (Method)Enum.Parse(typeof(Method), method, true);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Method '{method} is not supported.'");
            }
        }

        
    }
}
