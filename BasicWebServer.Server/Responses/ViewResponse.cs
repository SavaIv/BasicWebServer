using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Responses
{
    internal class ViewResponse : ContentResponse
    {
        private const char PathSeparator = '/';

        public ViewResponse(string viewName, string controllerName) 
            : base("", ContentType.Html)
        {
            // трябва да проверим как е структурирано името на вюто - защото е възможно да бъде подаен целия път до вюто,
            // а може да бъде подадено и само името на вюто (в url-то). Целта е да вземем (To get) the HTML from our views,
            // we will need the full path to them. By convention, each view has the .cshtml file extension and is accessed on:
            // "…/Views/{controllerName}/{viewName}.cshtml". 
            if (!viewName.Contains(PathSeparator))
            {
                // т.е. ако във viewName няма PathSeparator - ще си конструираме съответния в viewName така:
                viewName = controllerName + PathSeparator + viewName;
            }
            
            // след като сме уточнили viewName-a -> можем да си билднем ПЪЛНИЯ път до нашето вю:
            var viewPath = Path.GetFullPath($"./Views/" + viewName.TrimStart(PathSeparator) + ".cshtml");
            // Path.GetFullPath --> Path e системна функция, а GetFullPath е някакъв метод на тази функция
            // . <-- точката означава текуща директория. т.е. търсим от текущата директория - папка, която се казва вю
            // след което залепваме viewName-a: viewName.TrimStart(PathSeparator)... и най-накрая залепваме разширенито .cshml

            // вече можем да изчетем самия контент
            // Finally, we should read the view file content as a text and add it to the response body:
            var viewContent = File.ReadAllText(viewPath);

            // виж родителя : ContentResponse
            Body = viewContent;
        }
    }
}
