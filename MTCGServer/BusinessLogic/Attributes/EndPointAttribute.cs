using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class EndPointAttribute : Attribute
    {
        // public variables
        public HttpMethods Method { get; set; }
        public string Resource { get; set; }

        // constructor
        public EndPointAttribute(HttpMethods method, string resource)
        {
            Method = method;
            Resource = resource;
        }
    }
}
