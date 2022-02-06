using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Attributes
{
    public abstract class HttpMethodAttribute : Attribute
    {
        public HttpMethodAttribute(Method httpMethod)
        {
            HttpMethod = httpMethod;
        }

        // този тип Method е в BasicWebServer.Server/HTTP
        public Method HttpMethod { get; }
    }
}
