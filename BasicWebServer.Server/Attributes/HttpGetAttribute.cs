using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Attributes
{    
    public class HttpGetAttribute : HttpMethodAttribute
    {
        public HttpGetAttribute() : base(Method.Get)
        {
        }
    }
}
