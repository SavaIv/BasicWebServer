using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace BasicWebServer.Demo.Controllers
{
    public class HomeController : Controller
    {
        private const string HtmlForm = @"<form action='/HTML' method='POST'>
            Name: <input type='text' name='Name'/>
            Age: <input type='number' name ='Age'/>
            <input type='submit' value ='Save' />
        </form>";

        private const string DownloadForm = @"<form action='/Content' method='POST'>
            <input type='submit' value ='Download Sites Content' /> 
        </form>";

        private const string FileName = "content.txt";

        public HomeController(Request request) : base(request)
        {
        }

        // .MapGet("/", new TextResponse("Hello from the server!"))
        public Response Index() => Text("Hello from the server!");
        public Response Redirect() => Redirect("https://softuni.org");
        public Response Html() => Html(HomeController.HtmlForm);
        public Response HtmlFromPost()
        {
            string formData = string.Empty;

            // Form е Dictionary в Request класа, което се получва от ParseForm метода в същия клас - в Dictionary-то има
            // двоки ключ-стойност, където са записани информацията от ФОРМ-ата
            foreach (var (key, value) in Request.Form)
            {
                formData += $"{key} - {value}";
                formData += Environment.NewLine;
            }

            return Text(formData);
        }
        public Response Content() => Html(HomeController.DownloadForm);
        public Response DownloadContent()
        {
            DownloadSitesAsTextFile(
                HomeController.FileName,
                new string[] { "https://judge.softuni.org/", "https://softuni.org/" })
                .Wait();

            return File(HomeController.FileName);
        }
        public Response Cookies()
        {
            if (Request.Cookies.Any(c => c.Name != BasicWebServer.Server.HTTP.Session.SessionCookieName))
            {
                var cookieText = new StringBuilder();
                cookieText.AppendLine("<h1>Cookies</h1>");

                cookieText.Append("<table border='1'><tr><th>Name</th><th>Value</th></tr>");

                foreach (var cookie in Request.Cookies)
                {
                    cookieText.Append("<tr>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");
                    cookieText.Append("</tr>");
                }
                cookieText.Append("</table>");

                return Html(cookieText.ToString());
            }

            var cookies = new CookieCollection();
            cookies.Add("My-Cookie", "My-Value");
            cookies.Add("My-Second-Cookie", "My-Second-Value");
            
            return Html("<h1>Cookies set!</h1>", cookies);            
        }
        public Response Session()
        {
            string CurrentDateKey = "CurrentDate";
            bool sessionExists = Request.Session.ContainsKey(CurrentDateKey);
               
            if (sessionExists)
            {
                var currentDate = Request.Session[CurrentDateKey];

                return Text($"Stored date: {currentDate}!");
            }            

            return Text("Current date stored!");
        }


        // the DownloadWebSiteContent(string url) method, which should get the first 2000 symbols
        // of the HTML content of a site on a given URL
        private static async Task<string> DownloadWebSiteContent(string url)
        {
            // To get the content, we should send a "GET" request to the site and read its content.
            // For this reason, we will use the HttpClient class, which sends HTTP requests and receives HTTP responses
            // from a resource, identified by a URL. The class provides us with the GetAsync(string requestUri)
            // and the ReadAsStringAsync() methods. 

            var httpClient = new HttpClient();
            using (httpClient)
            {
                var response = await httpClient.GetAsync(url);

                var html = await response.Content.ReadAsStringAsync();

                // At the end, return only part of the HTML content, so that the result file is not too big
                return html.Substring(0, 2000);
            }
        }


        private static async Task DownloadSitesAsTextFile(string filename, string[] urls)
        {
            // a collection of type Task<string>, which holds the tasks for getting the HTML content from the sites
            // свалянето е асинхронно. в Лист-а ще добавяме Таскове. Забележи, че отпред няма await (въпреки, че мотода е async)
            // изчакването на тези Такове става при var responses = await Task.WhenAll(downloads); (след foreach-a)
            var downloads = new List<Task<string>>();

            foreach (var url in urls)
            {
                downloads.Add(DownloadWebSiteContent(url));
            }


            // Напрактика още със стартирането на сървъра се изпълнява този метод т.е. изтегля си информация от 
            // двата сайта и се "напълва" File
            await DownloadSitesAsTextFile(HomeController.FileName,
                new string[] { "https://judge.softuni.org/", "https://softuni.org/" });

            // Wait for all tasks to be executed together (in parallel) and get the result like this:
            // WhenAll - значи, че ще изчакаме да приключат всички Таскове, които са е Лист-а (downloads e Лист)
            string[] responses = await Task.WhenAll(downloads);

            // Join all the content from the responses in a way you want and get the result.
            var responsesString = string.Join(Environment.NewLine + new String('-', 100), responses);

            // Finally, use the File class to write the HTML content of the sites to a file with a given name asynchronously
            System.IO.File.WriteAllTextAsync(filename, responsesString);
        }

    }
}
