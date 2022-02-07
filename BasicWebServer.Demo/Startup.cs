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
            // !!! Метода Map връща IRoutingTable (т.е. RoutingTable) -> това дава възможност да извършим чейнването
            // при инстанцирането на HttpServer-a (това, което е направено по-долу)
            // В този случай викаме МаpGet -> който връща Map -> Map-a връща текущата инстанция на RoutingTable
            // ЧЕЙНВАНЕ Е ВЪЗМОЖНО, КОГАТО МЕТОД(ИТЕ) ВРЪЩАТ СЪЩИЯ ОБЕКТ, ОТ КОЙТО СА БИЛИ ИЗВИКАН(И) - 
            // ТАКА МОЖЕ, ОТНОВО ДА СЕ ИЗВИКА МЕТОД ОТ ТОЗИ ОБЕКТ (метода Map от обекта RoutingTable, връща this (RoutingTable))

            // при инстанциранрто на HttpServer се извършва "напълването" на едно Дикшинъри, където се мапва Рекуеста с
            // метода, който следва да го обработи. Ето как се подават нещата:
            //  - MapGet<ПодавамеТипаНаКонтролера>, като първи параметър на MapGet ще бъде подадена съответната инстанция на
            //    routingTable - подава се под името "routes" (но, не ние ще подадем този първи параметър, а фреймУорка).
            //    Това е така, защото това е екстеншън метод и това се прави автоматично (виж обясненията в
            //    RoutingTableExtensions класа). След това се подава URL-то (което е струинг path) и най-накрая подаваме делгата.
            //    Делегата съответно, ще бъде превърнат в друг вид делагат (виж RoutingTableExtensions класа) за да може да се
            //    подаде вече на MapGet метода от RoutingTable, който от своя страна да се подаде на RoutingTable.Мар метода,
            //    който от своя страна пък да запише този делгат в Дикшинърито route + Map метода връща "return this" т.е.
            //    връща текущата (и упдеътната) инстанцията на routingTable (т.е. ъпдейтнато Дикшинъри) - това е с цел да може
            //    да направим този чейнинг по-долу.
            // Чеининга не е задължителен. В смисъл, че ако Мар метода невръща return this; - най-накрая в кода си, ние пак
            // можем да напълним дикшинърито на routingTable ето така:
            //new HttpServer(routes =>
            //{
            //    routes.MapGet<HomeController>("/", c => c.Index());
            //    routes.MapGet<HomeController>("/Redirect", c => c.Redirect());
            //});

            var server = new HttpServer(routes => routes
            //    .MapGet<HomeController>("/", c => c.Index())  // връщаме съответен екшън, за целта се ползва се делегат
            //    .MapGet<HomeController>("/Redirect", c => c.Redirect())
            //    .MapGet<HomeController>("/HTML", c => c.Html())
            //    .MapPost<HomeController>("/HTML", c => c.HtmlFormPost())
            //    .MapGet<HomeController>("/Content", c => c.Content())
            //    .MapPost<HomeController>("/Content", c => c.DownloadContent())
            //    .MapGet<HomeController>("/Cookies", c => c.Cookies())
            //    .MapGet<HomeController>("/Session", c => c.Session())
            //    .MapGet<UsersController>("/Login", c => c.Login())
            //    .MapPost<UsersController>("/Login", c => c.LogInUser())
            //    .MapGet<UsersController>("/Logout", c => c.Logout())
            //    .MapGet<UsersController>("/UserProfile", c => c.GetUserData()));
            .MapControllers());


            await server.Start();
        }
    }
}