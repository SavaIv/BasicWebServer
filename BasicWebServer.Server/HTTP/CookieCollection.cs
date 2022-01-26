using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.HTTP
{
    // IEnumerable<Cookie> има за цел да усигури, Cookie-тата да можем да ги foreach-ваме
    public class CookieCollection : IEnumerable<Cookie>
    {
        private readonly Dictionary<string, Cookie> cookies;

        public CookieCollection()
        {
            cookies = new Dictionary<string, Cookie>();
        }
              

        public string this[string name] => cookies[name].Value;

        public void Add(string name, string value) => cookies[name] = new Cookie(name, value);
        // добавяме ново куким, където ключа е: cookies[name], а value-то е: new Cookie(name, value)

        public bool Contains(string name) => cookies.ContainsKey(name);

        public IEnumerator<Cookie> GetEnumerator()
        {
            return cookies.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           return GetEnumerator();
        }
    }
}
