using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Controllers
{
    public static class RoutingTableExtensions
    {
        // идеята на Extension класовете е open-close принципа. класа трябва да е затворен за промяна и отворени за разширяване
        // на практика самия RoutingTable няма да бъде променен, но чрез външен клас ще му дбавяме функционалност към него
        // този Extension клас трябва да е публичен и статични 

        // метода е generic, a конкретно защо е TController - с цел яснота (да знаем за какво става дума)
        // първия параметър на този метод ще е this КласаКойтоЕкстендваме (в случея е интерфейса)
        public static IRoutingTable MapGet<TController>(
            this IRoutingTable routingTable, string path, Func<TController, Response> controllerFunction)
            where TController : Controller
        => routingTable.MapGet(path, request => controllerFunction(CreateController<TController>(request)));

        public static IRoutingTable MapPost<TController>(
            this IRoutingTable routingTable, string path, Func<TController, Response> controllerFunction)
            where TController : Controller
        => routingTable.MapPost(path, request => controllerFunction(CreateController<TController>(request)));

        // какво връща функцията по-долу:
        // първо кастваме към (ТController). След което ползваме едно нещо, което се казва Activator (.CreateInstance)
        // Нашия клас Controller (виж BasicWebServer.Server/Controllers/Controller) няма празен конструктор, т.е.трябва да извикаме
        // конструтор, който е с параметър -> това става, като на .CreateInstance се подаде втори параметър (където са всичките
        // параметри, затова се подават като масив -> new[] { request } ... в случея в този масив ще има само един елемент - това
        // ще е request
        private static TController CreateController<TController>(Request request)
            => (TController)Activator.CreateInstance(typeof(TController), new[] { request });
    }
}
