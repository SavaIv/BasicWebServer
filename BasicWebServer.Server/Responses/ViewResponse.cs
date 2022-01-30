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

        public ViewResponse(string viewName, string controllerName, object model = null)
            : base("", ContentType.Html)
        {
            // object model = null -> това ще ни позволи да ползваме модели във вютата си. По дефолт е null т.е. може и без
            // него. После (по-надолу в кода) ще проверим дали от контролера идва model за да го ползваме.
            // Понеже незнаем какъв точно ще е модела - затова го подаваме в конструктора като object

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

            if (model != null)
            {
                viewContent = this.PopulateModel(viewContent, model);
            }

            // виж родителя : ContentResponse
            Body = viewContent;
        }

        // With the PopulateModel(string viewContent, object model) we will modify the content from the view. 
        private string PopulateModel(string viewContent, object model)
        {
            var data = model
                        .GetType()          // трябва да разберем какво ни е подадено
                        .GetProperties()    // вземаме пропъртитата на модела. Напрактика имеме КОЛЕКЦИЯ от тях
                        .Select(pr => new   // правим една проекция на тази КОЕКЦИЯ:   (pr идва от property)
                        {                   // правим един анонимен обект (удобно е), който е направен от двойки ключ-стойност
                            pr.Name,
                            Value = pr.GetValue(model) // <-- подава се инстанцията откято да се вземе съответната стойност                            
                        });
            // Какво правим всъщност:
            // 1. Вземаме типа на модела    2. Изчитаме пропъртитата 
            // 3. Итерираме колекцията с пропъртитата - за всяко едно пропърти, създаваме нов 
            //    анонимен обект в който ще имаме ИМЕ, което е името на пропъртито (pr.Name) и стойност, която е стойноста на
            //    пропъртито в подадения ни модел. Името на пропъртито го вземаме директно (лесно е): pr.Name, но за да
            //    вземем стоиността на пропъртито се налага малко рефлекшън: Value = pr.GetValue(model)
            //    * - ако не се даде име - името се взема от там от където се чете - както е случея с pr.Name. 
            //        но за Value-то - на практика няме "Value" - затова даваме Value = pr.GetValue(model)

            // след като имаме пропъртитата, трабва да заместиме placeHolder-ите във вюто -> те са обознавени с {{НякаквоИме}}
            foreach (var entry in data)
            {
                const string openingBrackets = "{{";
                const string closeingBrackets = "}}";

                // напрактика, тези скоби, поставени така е невъзможно да бъдат сбъркани в html-a т.е. може да са 
                // "насочващи" символи, къде следва да имам вмъкване на нещо наше -> т.е. срещаме ги -> и ги заменяме

                viewContent = viewContent.Replace($"{openingBrackets}{entry.Name}{closeingBrackets}", entry.Value.ToString());                               
            }

            return viewContent;
        }
    }
}
