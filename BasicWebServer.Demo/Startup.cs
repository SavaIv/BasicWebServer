using BasicWebServer.Server;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;
using System.Text;
using System.Web;

namespace BasicWebServer.Demo
{
    public class Startup
    {
        // Create the form as a string constant and use it as a part of an HTML response
        private const string HtmlForm = @"<form action='/HTML' method='POST'>
            Name: <input type='text' name='Name'/>
            Age: <input type='number' name ='Age'/>
            <input type='submit' value ='Save' />
        </form>";

        // това е html-a, когато искаме да даунлодваме ->
        // имаме Даунлод бутон за да тригърнем the action on the "/Content" page with method "POST":
        private const string DownloadForm = @"<form action='/Content' method='POST'>
            <input type='submit' value ='Download Sites Content' /> 
        </form>";

        private const string FileName = "content.txt";

        private const string LoginForm = @"<form action='/Login' method='POST'>
            Username: <input type='text' name='Username'/>
            Password: <input type='text' name='Password'/>
            <input type='submit' value ='Log In' /> 
        </form>";

        private const string Username = "user";

        private const string Password = "user123";

        public static async Task Main()
        {
            await DownloadSitesAsTextFile(Startup.FileName,
                new string[] { "https://judge.softuni.org/", "https://softuni.org/" });

            var server = new HttpServer(routes => routes
                .MapGet("/", new TextResponse("Hello from the server!"))
                .MapGet("/Redirect", new RedirectResponse("https://softuni.org/"))
                .MapGet("/HTML", new HtmlResponse(Startup.HtmlForm))                
                .MapPost("/HTML", new TextResponse("", Startup.AddFormDataAction))
                .MapGet("/Content", new HtmlResponse(Startup.DownloadForm))
                .MapPost("/Content", new TextFileResponse(Startup.FileName))
                .MapGet("/Cookies", new HtmlResponse("", Startup.AddCookiesAction))
                .MapGet("/Session", new TextResponse("", Startup.DisplaySessionInfoAction))
                .MapGet("/Login", new HtmlResponse(Startup.LoginForm)));
                
            await server.Start();
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

        // ето така we can add an action to be executed before the response is returned
        // т.е. we will add the form data to the response body:
        private static void AddFormDataAction(Request request, Response response)
        {
            response.Body = "";

            foreach (var (key, value) in request.Form)
            {
                response.Body += $"{key} - {value}";
                response.Body += Environment.NewLine;
            }
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

        // As we have created the "POST" request mapping to return a TextFileResponse with a file name,
        // we should fill in the file with data before the response is returned. To do this, we will create the
        // DownloadSitesAsTextFile(…) method in the Startup class and invoke it before the mappings are created.
        // With this method, we will get the HTML content of several sites and write it to a text file with a given name.
        //  - the DownloadSitesAsTextFile() method, will downloads the content of given sites asynchronously.
        //  - Then, the whole HTML content will be joined and written to a single file.  
        //  - First, the method should accept a file name and a string array with URLs of sites to be downloaded:
        private static async Task DownloadSitesAsTextFile(string filename, string[] urls)
        {
            // a collection of type Task<string>, which holds the tasks for getting the HTML content from the sites
            var downloads = new List<Task<string>>();

            foreach (var url in urls)
            {
                downloads.Add(DownloadWebSiteContent(url));
            }

            // Wait for all tasks to be executed together (in parallel) and get the result like this:
            var responses = await Task.WhenAll(downloads);

            // Join all the content from the responses in a way you want and get the result.
            var responsesString = string.Join(Environment.NewLine + new String('-', 100), responses);

            // Finally, use the File class to write the HTML content of the sites to a file with a given name asynchronously
            await File.WriteAllTextAsync(filename, responsesString);
        }
    }
}