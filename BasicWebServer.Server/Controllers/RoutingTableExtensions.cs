using BasicWebServer.Server.Attributes;
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

        // във връзка Data Binding-а правим този метод
        public static IRoutingTable MapControllers(this IRoutingTable routingTable)
        {
            // Трябва да си вземем екшъните (нашите рутинги - зависят от въпросните екшъни)
            IEnumerable<MethodInfo> controllerActions = GetControllerActions();

            foreach (var controllerAction in controllerActions)
            {
                // трябват ни името на контролера и името на екшъна за да можем да ги "извадим"
                string controllerName = controllerAction.DeclaringType
                                                        .Name       // името на контролреа (класа) ще е с "controller" накрая
                                                        .Replace(nameof(Controller), string.Empty);
                
                string actionName = controllerAction.Name;

                // след като имаме контролера и екшъна - можем да си сглобим пътя. Той ще ни е необходим за рутинг таблицата.
                string path = $"/{controllerName}/{actionName}";

                // трябва ни и респонс функцията, която да добавим отзад в МАП-а (която получава рекуест и връща респонс)
                var responseFunction = GetResponseFunction(controllerAction);

                // опитваме се да си мапнем таблицата. Kакво ни е нужно за таблицата? - трябва ни пътя, трябва ни verb-a
                // и ни трябва responseFunction-a.
                Method httpMethod = Method.Get;  // това е метод по подразбиране!!!

                // вземаме си къстъм атрибута (който си направихме)
                var actionMethodAttribute = controllerAction.GetCustomAttribute<HttpMethodAttribute>();

                // трябва да проверим дали съществува такъв атрибут (ако е нулл - ще си остане стойността по подразбиран)
                if (actionMethodAttribute != null)
                {
                    httpMethod = actionMethodAttribute.HttpMethod;
                }

                // трябва да мапнем
                routingTable.Map(httpMethod, path, responseFunction);
            }

            return routingTable;
        }

        //
        private static Func<Request, Response> GetResponseFunction(MethodInfo controllerAction)
            // трябва да върнем Func<Request, Response>. "=>" е ретърн т.е. казваме => и започваме да си правим Funk-a
        => request =>
            // т.е. връщаме нова функция, която получава рекуест... и започва да прави нещо:
        {
            // Трябва да си създадем инстанция на нашия контролер - ще ни е нужна за да може да намерим функцията
            var controllerInstance = CreateController(controllerAction.DeclaringType, request);

            // Трябва да намерин всички параметри на нашия екшън (ЕТО ТУК Е ПРОБЛЕМА, ЕТО ЗАТОВА ПИШЕМ ТОЗИ ЦЕЛИЯ ЗАСУКАН КОД
            // за да може на екшъните да подаваме параметри. Съответно да вземаме тези параметри отнякъде т.е. да се получи
            // data binding --> dat-та, която е в рекуеста да се bind-не към самия екшън. Това беше целта -> да може на
            // екшъните да подаваме параметри и съответно да вземаме от някъде тези параметри. Това е най-трудната част
            var parameterValues = GetParameterValues(controllerAction, request);
            // след това ще се опитаме да върнем резултат. Резултата, който ще върнем след като върнем параметрите ще е:
            return (Response)controllerAction.Invoke(controllerInstance, parameterValues);
            // т.е. резултата на нашата функция ще бъде изпълнението на този метод:
            // (Response)controllerAction.Invoke(controllerInstance, parameterValues);
            // -> изпълняваме controllerAction-а, като трябва да подадем инстанция, върху която го изпълняваме - инстанцията
            // е: controllerInstanc. И параметрите с които го изплняваме: parameterValues
        };

        private static object[] GetParameterValues(MethodInfo controllerAction, Request request)
        {
            // целта е да вземем оbject[] масив от обекти
            // 1. ще вземем всички параметри на екшъна за да можем да им намерим value-тата
            var actionParameters = controllerAction.GetParameters()
            // това, че сме взели параметрите като parameterInfo, ни ни грее много, защото те не ни трябват като parameterInfo
            // (това, което ще получим от горния ред код). Затова parameterInfo-то ще селектнем нов анонимен (щото сме мързеливи) обект:
            .Select(p => new
            {
                p.Name,
                p.ParameterType
            })
            .ToArray();

            // Вече имеме actionParameters -> масив от name и parameterType <- какво правим с тях?
            // 2. ще си направим това, което смятаме да връщаме --> което е масив от parameter values
            var parameterValues = new object[actionParameters.Length];

            // ще трябва да превъртим параметрите - и ще попълваме value-тата
            for (int i = 0; i < actionParameters.Length; i++)
            {
                // първо трябва да провериме типа. Когато говорим за Data Binding - имаме два варианта 
                // единия вариянт е да е някакъв прост тип - тогава байндваме директно 
                // обаче ако става дума за клас - ние ще трябва да инстанцираме този клас и да Баинднем всеки един негов параметър
                // ще разгледаме двата случея - единия, когато имаме прост тип, и по-сложния вариянт
                var parameter = actionParameters[i];

                // проверяваме типа
                if (parameter.ParameterType.IsPrimitive 
                    || parameter.ParameterType == typeof(string))  // ако имаме прост тип или стринг - директно го asign-ваме
                {
                    string parameterValue = request.GetValue(parameter.Name);
                    // записваме го в реалния тип на екшън параметъра
                    parameterValues[i] = Convert.ChangeType(parameterValues, parameter.ParameterType);
                    // ChangeType очаква да види самата стойност и към какво да я конвертира - след което я записва в масива parameterValues
                }
                else  // ако параметъра не е примитивен тип - нещата са малко по-сложни и ще трябва да поработим малко
                {
                    // налага се да си създадем съответната инстанция на параметъра (защото той не е прост тип)
                    var parameterValue = Activator.CreateInstance(parameter.ParameterType);

                    // след като сме си създали инстанцията ще трябва да вземем всичките параметри (пропъртита) на Параметъра
                    // който инстанцирахме на горния ред код. Звучи тъпо "вземаме параметрите на параметъра" :) - за това,
                    // за да не звучи така ще кажем, че ще вземаме пропъртита (вместо параметри) за блгозвучие
                    var parameterProperties = parameter.ParameterType.GetProperties();
                    // Така вземаме всички пропъртита.
                    
                    // Сега трябва да им дадем стойности на пропъртитата -> в цикъл ще се опитаме да получим value-то:
                    foreach (var property in parameterProperties)
                    {
                        // за всяко едно от пропъртитата - вземаме стойността на пропъртито
                        var propertyValue = request.GetValue(property.Name);
                        // после го сетваме така:
                        property.SetValue(parameterValue, Convert.ChangeType(propertyValue, property.PropertyType));
                        // където: parameterValue е инстанцията върху която го сетваме 
                        // вземаме: propertyValue-то и PropertyType-а
                    }

                    // След като foreach-a премине - можем да добавим нашата инстанция т.е. propertyValue-то
                    parameterValues[i] = parameterValue;
                }
            }

            return parameterValues;
        }

        private static string GetValue(this Request request, string? name)
            => request.Query.GetValueOrDefault(name) ?? request.Form.GetValueOrDefault(name);
        // ако намери стойността от Query-то ще я върне, ако ли не -> ще пробва да вземе стойност от формата
        // ако имаме едни и същи имена и във Формата и в Queri-то ... не е ОК (неправи такива неща)

        // целта е да вземем само контолерите -> от тях трябва да вземем екшъните
        private static IEnumerable<MethodInfo> GetControllerActions() // можем да полазваме LINQ и директно да върнем стойност
             => Assembly            // <- това ни дава възможност да работим деректно с рефлекшън
            .GetEntryAssembly()     // <- вземаме стартиращото асембли (асемблито, което е започнало)
            .GetExportedTypes()     // <- Вземаме негпвите типове. GetExportedTypes защото вземаме само публичните типове
            .Where(t => t.IsAbstract == false)   // филтрираме за да сме сигурни, че вземаме само контролерите. Типа трябва да не е абстрактен
            .Where(t => t.IsAssignableTo(typeof(Controller))) // трябва да наследява типа контролер
            .Where(t => t.Name.EndsWith(nameof(Controller))) // където името дали завършва на "Controller"
             // до тук вземахме (само) контролерите. сега от тях трябва да вземем всички екшъни ще ползваме SelectMany, 
            .SelectMany(t => t
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)  //  където казаваме "Дай ми всички методи",
                .Where(m => m.ReturnType.IsAssignableTo(typeof(Response)))  
                 // искаме да вземем само екшъните, защото може може да сме захапали и някой друг публичен инстанционен метод
            ).ToList();

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
