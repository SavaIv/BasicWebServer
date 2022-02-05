using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Controllers
{
    public static class RoutingTableExtensions
    {
        // идеята на Extension класовете е open-close принципа. класа трябва да е затворен за промяна и отворени за разширяване
        // на практика самия RoutingTable няма да бъде променен, но чрез външен клас ще му дбавяме функционалност към него
        // този Extension клас трябва да е публичен и статични 

        // метода е generic, изписваме "TController" вместо само "Т", с цел яснота (да знаем за какво става дума). Не е проблем.
        // Има условия, които трябва да се спазят, когато се пише extension - и това са:
        // 1. Екстеншън метода винаги е СТАТИЧЕН и винаги се намира в СТАТИЧЕН клас - това е така, защото напрактика работим с
        //    инстанция на клас, към който добавяме допълнителен метод (няма как да промени самия клас, но ще му добавим нещо).
        //    Освен това искаме това (новодобавеното) да го има винаги и навсякъде, а не да е в конкретна инстанция.
        //    За да можем да закачим (това новото), то трябва да е метод на класа т.е. да е статичен.
        // 2. Първия параметър на този екстеншън метод, трябва да е конкретната инстанция, която разширяваме -> по принцип,
        //    когато ще викаме този екстеншън метод, ние няма да го подаваме този параметър, НО в дифиницията на екстеншън
        //    метода, този параметър е изписан на първо място - това в нашия случай е this IRoutingTable routingTable
        //    т.е. разширяваме конкретната инстанция - задължително трянва да има this (този път VS не избледнва this-a) и да се
        //    укаже инстанцията. В тази връзка името на класа (RoutingTableExtensions) няма никакво значение - важно е какво ще
        //    изпишем тук, като първи параметър. Още веднъж: Ние няма да го подаваме този параметър - фремУорка ще го подаде
        //    автоматични. Напрактика, когато екстендваме - ние казваме на компилатора, че ще екстендваме, като подаваме това:
        //    this IRoutingTable routingTable, като първи параметър - от този this, компилатора ще разбере че искаме да 
        //    екстендваме. НЕ СЕ БЪРКАЙ С НАСЛЕДЯВАНИЯ-МАСЛЕДЯВАНИЯ - нищо нетрябва да се наследява!
        //    В един и същи клас може да се екстнднат различни класове - няма проблем.
        //    Останалите параметри са си наши (string path и Func<TController, Response> controllerFunction). 
        // Какво прави този екстешън метод? -> вика метода MapGet на routingTable (инстанциата която влиза в метода
        // като първи параметър - та, на тази инстанция ще извика нейния MagGet). Тук идва врътката, защото routingTable.MapGet
        // иска за праметър Func<Request, Response>, а ние искаме да и подадем Func<TController, Response> т.е. единия тип 
        // делегат трябва да го превърнем в другия тип делегат. Как го правим това нещо? - Ще изпълним тази функция:
        // request => controllerFunction(CreateController<TController>(request))
        // напрактика създаваме нов делегат -> ламбдата е делегат (имаме параметър "х", казаме какво връща "=>" и можем да
        // правим разни неща с този параметър. Примерно: x => x.y = 6). Та, този нов делагат, който си правим, ще получи като
        // параметър "request" и като резултат ще върне резултата от изпълнението на функцията 
        // controllerFunction връща респонс т.е. ние сме създали делгат, който ще получи рекуест, а ще върне респонс
        // Какво конкретно прави този делегат? - Извиква controllerFunction - като вътре в нея подава самия контролер, който
        // трябва да бъде изпълнен. Как подава контролера? - Подава го, като му създава инстанция (създавайки контролер) -> 
        // затова има метод, който създава контролера: private static TController CreateController<TController>(Request request)
        // с помощта на на малко рефлекшен (методи от .НЕТ)

        public static IRoutingTable MapGet<TController>(
            this IRoutingTable routingTable, string path, Func<TController, Response> controllerFunction)
            where TController : Controller
        => routingTable.MapGet(path, request => controllerFunction(CreateController<TController>(request)));
        // Func<TController, Response> controllerFunction - това не е просто делегат, който иска рекуест за да върне респонс.
        // Понеже имитираме MVC - Funk очаква да получи контролер (инстанция на контролер) и ще връща респонс.
        // Имаме ограничението: TController : Controller --> целта на това условие е да ни пази от грешки -> т.е. подсигуряваме
        // се че, нашата функция ще работи правилно.      

        public static IRoutingTable MapPost<TController>(
            this IRoutingTable routingTable, string path, Func<TController, Response> controllerFunction)
            where TController : Controller
        => routingTable.MapPost(path, request => controllerFunction(CreateController<TController>(request)));

        // какво връща функцията по-долу:
        // първо кастваме към (ТController). След което ползваме едно нещо, което се казва Activator (.CreateInstance)
        // Нашия клас Controller (виж BasicWebServer.Server/Controllers/Controller) няма празен конструктор, т.е.трябва да извикаме
        // конструтор, който е с параметър -> това става, като на .CreateInstance се подаде втори параметър (където са всичките
        // параметри, на конструктора, който ще бъде извикан - затова се подават като масив -> new[] { request } ...
        // в случея в този масив ще има само един елемент - това ще е request
        // Именно тук е мястото където може да стане проблем - затова е именно онзи констреинт: where TController : Controller
        // който го има в методите по-горе. Ако този констрейнт го няма, то няма да можем да подадем втория параметър в
        // метода по-долу ( new[] { request } ) и опита ни за рефлекшън ще гърми.
        // НАПРАКТИ: Този метод по-долу, превръща делгата Func<TController, Response> в делгата Func<Request, Response>
        private static TController CreateController<TController>(Request request)
            => (TController)Activator.CreateInstance(typeof(TController), new[] { request });

        // имаме нужда от метод който да ни върши работа за serviceCollection-а
        private static Controller CreateController(Type controllerType, Request request)
        {
            // правим си инстанция на контрилера, като инстанцията я вземаме от нашия inversion Of Control Kонтейнер
            // който се намира в                .ServiceCollection
            var controller = (Controller)Request.ServiceCollection.CreateInstance(controllerType);
            // използваме метода, който си създадохме за да направим инстанцията на контролера
            // сега имаме проблем - знаем, че контролера очаква Request, a в дадения случай, CreateInstance ще инстанцира
            // Рекуеста вместо нас т.е. ще инстанцира някакъв празен Рекуест (което не е ОК). За целта:
            // ние знаем типа на контроилера и ще се опитаме да си вземем рекуеста
            controllerType
                .GetProperty("Request", BindingFlags.Instance | BindingFlags.NonPublic) // bindingFlags са bit флагове
                .SetValue(controller, request);                                         // битовото или "|" промяня стойности
            // с горния код, инстанцирахме конролера, но след това намерихме вътре пропъртито Рекуест и му заместихме 
            // стойността, защото иначе няма да е вярна

            return controller;
            // вече, ако в някой от контролерите, в конструкторите им, бъде добавен сървис -> то той ще бъдат автоматично 
            // инстанциран -> съответно този сървис трябва да бъде описан.
        }

    }
}
