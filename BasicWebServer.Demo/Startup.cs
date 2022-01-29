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
        public static async Task Main()
        {            
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
    }
}