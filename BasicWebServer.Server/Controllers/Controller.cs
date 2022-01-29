using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Controllers
{
    public class Controller
    {
        public Controller(Request request)
        {
            Request = request;
        }

        protected Request Request { get; private init; }

        protected Response Text(string text) => new TextResponse(text);
        protected Response Html(string html, CookieCollection cookies = null)
        {
            var response = new HtmlResponse(html);

            if(cookies != null)
            {
                foreach(var cookie in cookies)
                {
                    response.Cookies.Add(cookie.Name, cookie.Value);
                }
            }

            return response;
        }
        protected Response BadRequest() => new BadRequestResponse();
        protected Response Unauthorized() => new UnauthorizedResponse();
        protected Response NotFound() => new NotFoundResponse();
        protected Response Redirect(string location) => new RedirectResponse(location);
        protected Response File(string fileName) => new TextFileResponse(fileName);

        // модификация на контролера за да може да връща вю. Ще има малко рефлекшън.
        // за да можем да си свършим работата ни е нужно имато на вюто - имаме един атрибут: [CallerMemberName], който
        // ни помага да вземем кой е извикал съответния метод т.е. "от кой метод е бил извикан този метод". Понеже имената
        // на вютата и екшъните съвпадат (такава е конвенцията) т.е. ще търсим такива вюта, които съвпадат със съответния
        // контролер. Обаче имаме малък проблем - ще трябва да се "поизчистят" имената където има "controller" т.е. трябва ни
        // името на контролера, но без думичката "controller" в името -> за целта ще си направим метод GetControllerName().
        // The GetControllerName() method gets the controller name, without the "Controller" part
        // (for example "HomeController" --> "Home"):

        // говорейки си за [CallerMemberName] -> викащия метод ще е някой от методите в HomeController или UsersController =>
        // и името на вюто ще трябва да е същото (DownloadContent, Cookies, Session и т.н.).
        // [CallerMemberName] e атрибут -> По принцип атрибута позволява да получим някаква допълнителна възножност ->
        // в случея атрибута каза на компилатора от къде да си вземе това, което му трябва.
        protected Response View([CallerMemberName] string viewName = "")
            => new ViewResponse(viewName, GetControllerName()); // <-- ще се създаде нов ViewResponse

        // това е метода, който маха "controller" името на контролера:
        // По повод на контролера -> типа на нашия клас ще е някой от наследниците - примерно HomeController или UsersController
        // и като премахнем от името думата "controller" - ще получим това, което ни трябва.
        private string GetControllerName() => this.GetType().Name.Replace(nameof(Controller), string.Empty);
        // .GetType() - ще върне тип на контролера в който се намираме в момента 
    }
}
