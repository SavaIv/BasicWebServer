using BasicWebServer.Server.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Attributes
{
    public class HttpPostAttribute : HttpMethodAttribute
    {
        public HttpPostAttribute() : base(Method.Post)
        {
        }
    }
}
