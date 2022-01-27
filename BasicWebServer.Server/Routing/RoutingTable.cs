using BasicWebServer.Server.Common;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;

namespace BasicWebServer.Server.Routing
{
    public class RoutingTable : IRoutingTable
    {
        private readonly Dictionary<Method, Dictionary<string, Func<Request, Response>>> routes;

        //public RoutingTable() =>
        //    this.routes = new Dictionary<Method, Dictionary<string, Response>>()
        //    {
        //        [Method.Get] = new Dictionary<string, Response>(),
        //        [Method.Post] = new Dictionary<string, Response>(),
        //        [Method.Put] = new Dictionary<string, Response>(),
        //        [Method.Delete] = new Dictionary<string, Response>()
        //    };

        public RoutingTable() => this.routes = new()
        {
            // StringComparer.InvariantCultureIgnoreCase  <-- игнорира главните букви
            [Method.Get] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Post] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Put] = new(StringComparer.InvariantCultureIgnoreCase),
            [Method.Delete] = new(StringComparer.InvariantCultureIgnoreCase)
        };

        public IRoutingTable Map(Method method, string path, Func<Request, Response> responseFunction)
        {
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
            // изкарали сме си функцията - var-a e делегат, който е Func от нещоСи
            // това, което се иска обаче е да върнем респонс. Правим го така:          
            return responseFunction(request);
            // това е така, защото делегата (Func<Request, Response>), получва като параметър Request и връща като 
            // резултат респонс
            // ????? тук нещо невдянах защо трябва да се подаде (request)
        }
    }
}
