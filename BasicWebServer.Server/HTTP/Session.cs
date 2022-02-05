using BasicWebServer.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.HTTP
{
    public class Session
    {
        // погледни в контролера (HomeController) как се ползва

        // това е името на кукито
        public const string SessionCookieName = "MyWebServerSID";
        // името на CurrentDateKey-я
        public const string SessionCurrentDateKey = "CurrentDate";
        // името на UserKey-я
        public const string SessionUserKey = "AuthenticatedUserId";
        // Правим си дикшинъри data, в което да си пълним разни неща
        private Dictionary<string, string> data;
        
        // когато инстанцираме нашата сесия - трябва да подадем на конструктъра някаквo id
        public Session(string id)
        {
            Guard.AgainstNull(id, nameof(id));

            Id = id;
            data = new Dictionary<string, string>();
        }

        // id-то е Inti, защото не бива да се пипа (променя) след като е създадена сесиата (класа) - това е Id-то на сесията
        public string Id { get; init; }

        // правим си един индексатор, за да може да вадим информация от дикшинърито в класа.
        // този this е текущата инстанция на този клас (Session) - малко рефлекшън
        // та, на session може отзад да слагаме квадратни скоби [] и да подаваме нещо (защото тя е уж някаква колекция)
        // в случея, обаче индекса неработи с числа, а с текст (string key).
        // ако някой извика: sessin["pesho"] = ...   <-- съответно ще бъде извикан set-ера и ще се сетне value-то (...)
        // при опит за четене: sessin["pesho"] <-- ще бъде извикан get-ера (и ще получим ...) стойността на "pesho"
        // а, get-a и set-a дърпат и пишат от/в дикшинърито.
        public string this[string key]
        {
            get => data[key];
            set => data[key] = value;
        }

        public bool ContainsKey(string key) => data.ContainsKey(key);

        public void Clear() => data.Clear();
    }
}
