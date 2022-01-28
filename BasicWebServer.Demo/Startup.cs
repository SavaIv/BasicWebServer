using BasicWebServer.Demo.Controllers;
using BasicWebServer.Server;
using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;
using System.Text;
using System.Web;

namespace BasicWebServer.Demo
{
    public class Startup
    {
        //// Create the form as a string constant and use it as a part of an HTML response
        //private const string HtmlForm = @"<form action='/HTML' method='POST'>
        //    Name: <input type='text' name='Name'/>
        //    Age: <input type='number' name ='Age'/>
        //    <input type='submit' value ='Save' />
        //</form>";

        // това е html-a, когато искаме да даунлодваме ->
        // имаме Даунлод бутон за да тригърнем the action on the "/Content" page with method "POST":
        //private const string DownloadForm = @"<form action='/Content' method='POST'>
        //    <input type='submit' value ='Download Sites Content' /> 
        //</form>";

        //private const string FileName = "content.txt";

        private const string LoginForm = @"<form action='/Login' method='POST'>
            Username: <input type='text' name='Username'/>
            Password: <input type='text' name='Password'/>
            <input type='submit' value ='Log In' /> 
        </form>";

        private const string Username = "user";

        private const string Password = "user123";

        public static async Task Main()
        {
            // Напрактика още със стартирането на сървъра се изпълнява този метод т.е. изтегля си информация от 
            // двата сайта и се "напълва" File
            // await DownloadSitesAsTextFile(Startup.FileName,
            //    new string[] { "https://judge.softuni.org/", "https://softuni.org/" });

            var server = new HttpServer(routes => routes
                .MapGet<HomeController>("/", c => c.Index())  // връщаме съответен екшън, за целта се ползва се делегат
                .MapGet<HomeController>("/Redirect", c => c.Redirect())
                .MapGet<HomeController>("/HTML", c => c.Html())
                .MapPost<HomeController>("/HTML", c => c.HtmlFromPost())
                .MapGet<HomeController>("/Content", c => c.Content())
                .MapPost<HomeController>("/Content", c => c.DownloadContent())
                .MapGet<HomeController>("/Cookies", c => c.Cookies())
                .MapGet<HomeController>("/Session", c => c.Session())
                .MapGet<UsersController>("/Login", c => c.Login())
                .MapPost<UsersController>("/Login", c => c.LogInUser())
                .MapGet<UsersController>("/Logout", c => c.Logout())
                .MapGet<UsersController>("/UserProfile", c => c.GetUserData()));

            await server.Start();   
        }

        private static void GetUserDataAction(Request request, Response response)
        {
            if (request.Session.ContainsKey(Session.SessionUserKey))
            {
                response.Body = "";
                response.Body += $"<h3>Currently logged-in user " + $"is with username '{Username}'</h3>";
            }
            else
            {
                response.Body = "";
                response.Body += $"<h3>You should first log in " + "- <a href='/Login'>Login</a></h3>";
            }
        }

        private static void LogoutAction(Request request, Response response)
        {
            request.Session.Clear();
            
            response.Body = "";
            response.Body += "<h3>Logged out successfully!</h3>";
        }

        private static void LoginAction(Request request, Response response)
        {
            request.Session.Clear();

            //var testSessionBeforeLogin = request.Session;

            var bodyText = "";

            var usernameMatches = request.Form["Username"] == Startup.Username;
            var passwordMatches = request.Form["Password"] == Startup.Password;

            if (usernameMatches && passwordMatches)
            {
                request.Session[Session.SessionUserKey] = "MyUserId";
                response.Cookies.Add(Session.SessionCookieName, request.Session.Id);

                bodyText = "<h3>Logged successfully!</h3>";

                //var testSessionAfterLogin = request.Session;
            }
            else
            {
                bodyText = Startup.LoginForm;
            }

            response.Body = "";
            response.Body += bodyText;
        }        

        private static void DisplaySessionInfoAction(Request request, Response response)
        {
            var sessionExists = request.Session.ContainsKey(Session.SessionCurrentDateKey);

            var bodyText = "";

            if (sessionExists)
            {
                var currentDate = request.Session[Session.SessionCurrentDateKey];
                bodyText = $"Stored date: {currentDate}!";
            }
            else
            {
                bodyText = "Current date stored!";
            }

            response.Body = "";
            response.Body = bodyText;
        }

        private static void AddCookiesAction(Request request, Response response)
        {
            var requestHasCookies = request.Cookies.Any(c => c.Name != Session.SessionCookieName);
            // c => c.Name != Session.SessionCookieName   <-- This is necessary because otherwise the session cookie
            //                                                 will prevent the creation of other cookies. 


            var bodyText = "";

            // If we have any cookies from the response, we should display them in HTML format.
            // If there aren't any, show the "No cookies yet!" message.
            // Modify the response body to change the response content:

            if (requestHasCookies)
            {
                var cookieText = new StringBuilder();
                cookieText.AppendLine("<h1>Cookies</h1>");

                cookieText.Append("<table border='1'><tr><th>Name</th><th>Value</th></tr>");

                foreach (var cookie in request.Cookies)
                {
                    cookieText.Append("<tr>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");
                    cookieText.Append("</tr>");
                }
                cookieText.Append("</table>");

                bodyText = cookieText.ToString();
            }
            else
            {
                bodyText = "<h1>Cookies set!</h1>";
            }

            if (!requestHasCookies)
            {
                response.Cookies.Add("My-Cookie", "My-Value");
                response.Cookies.Add("My-Second-Cookie", "My-Second-Value");
            }

            response.Body = bodyText;
        }

        //// ето така we can add an action to be executed before the response is returned
        //// т.е. we will add the form data to the response body:
        //private static void AddFormDataAction(Request request, Response response)
        //{
        //    response.Body = "";

        //    foreach (var (key, value) in request.Form)
        //    {
        //        response.Body += $"{key} - {value}";
        //        response.Body += Environment.NewLine;
        //    }
        //}

        //// the DownloadWebSiteContent(string url) method, which should get the first 2000 symbols
        //// of the HTML content of a site on a given URL
        //private static async Task<string> DownloadWebSiteContent(string url)
        //{
        //    // To get the content, we should send a "GET" request to the site and read its content.
        //    // For this reason, we will use the HttpClient class, which sends HTTP requests and receives HTTP responses
        //    // from a resource, identified by a URL. The class provides us with the GetAsync(string requestUri)
        //    // and the ReadAsStringAsync() methods. 

        //    var httpClient = new HttpClient();
        //    using (httpClient)
        //    {
        //        var response = await httpClient.GetAsync(url);

        //        var html = await response.Content.ReadAsStringAsync();

        //        // At the end, return only part of the HTML content, so that the result file is not too big
        //        return html.Substring(0, 2000);
        //    }
        //}

        //// As we have created the "POST" request mapping to return a TextFileResponse with a file name,
        //// we should fill in the file with data before the response is returned. To do this, we will create the
        //// DownloadSitesAsTextFile(…) method in the Startup class and invoke it before the mappings are created.
        //// With this method, we will get the HTML content of several sites and write it to a text file with a given name.
        ////  - the DownloadSitesAsTextFile() method, will downloads the content of given sites asynchronously.
        ////  - Then, the whole HTML content will be joined and written to a single file.  
        ////  - First, the method should accept a file name and a string array with URLs of sites to be downloaded:
        //private static async Task DownloadSitesAsTextFile(string filename, string[] urls)
        //{
        //    // a collection of type Task<string>, which holds the tasks for getting the HTML content from the sites
        //    // свалянето е асинхронно. в Лист-а ще добавяме Таскове. Забележи, че отпред няма await (въпреки, че мотода е async)
        //    // изчакването на тези Такове става при var responses = await Task.WhenAll(downloads); (след foreach-a)
        //    var downloads = new List<Task<string>>();

        //    foreach (var url in urls)
        //    {
        //        downloads.Add(DownloadWebSiteContent(url));
        //    }

        //    // Wait for all tasks to be executed together (in parallel) and get the result like this:
        //    // WhenAll - значи, че ще изчакаме да приключат всички Таскове, които са е Лист-а (downloads e Лист)
        //    string[] responses = await Task.WhenAll(downloads);    

        //    // Join all the content from the responses in a way you want and get the result.
        //    var responsesString = string.Join(Environment.NewLine + new String('-', 100), responses);

        //    // Finally, use the File class to write the HTML content of the sites to a file with a given name asynchronously
        //    await File.WriteAllTextAsync(filename, responsesString);
        //}
    }
}