using BasicWebServer.Server.HTTP;

namespace BasicWebServer.Server.Routing
{
    public interface IRoutingTable
    {
        // Обърни внимание, че метода Map връща IRoutingTable (т.е. RoutingTable) -> това дава възможност да извършим чейнването
        // при инстанцирането на HttpServer-a (виж Startup.cs, където е чейнванато: MapGet(bal).Mapget(blabla).Mapget(итн))
        // В този случай имаме следното: МаpGet -> връща Map -> Map-a връща текущата инстанция на RoutingTable
        // ЧЕЙНВАНЕ Е ВЪЗМОЖНО, КОГАТО МЕТОД(ИТЕ) ВРЪЩАТ СЪЩИЯ ОБЕКТ, ОТ КОЙТО СА БИЛИ ИЗВИКАН(И) - 
        // ТАКА МОЖЕ, ОТНОВО ДА СЕ ИЗВИКА МЕТОД ОТ ТОЗИ ОБЕКТ (метода Map от обекта RoutingTable, връща this (RoutingTable))
        IRoutingTable Map(Method method, string path, Func<Request, Response> responseFunction);
        IRoutingTable MapGet(string path, Func<Request, Response> responseFunction);
        IRoutingTable MapPost(string path, Func<Request, Response> responseFunction);
    }
}
