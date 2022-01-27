using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Demo.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(Request request) : base(request)
        {
        }

        // .MapGet("/", new TextResponse("Hello from the server!"))
        //public Response Index() => Text("Hello from the server!");
    }
}
