using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Network.HTTP
{
    public class HttpRequest
    {
        // public variables
        public HttpMethods Method { get; }
        public string[] Resource { get; }
        public string Body { get; }
        public string Token { get; }

        // constructor
        public HttpRequest(HttpMethods method, string[] resource, string token, string body)
        {
            Method = method;
            Resource = resource;
            Body = body;
            Token = token;
        }
        public HttpRequest(HttpMethods method, string[] resource, string token)
        {
            Method = method;
            Resource = resource;
            Body = "";
            Token = token;
        }
    }
}
