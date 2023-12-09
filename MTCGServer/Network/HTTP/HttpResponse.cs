using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Network.HTTP
{
    public class HttpResponse
    {
        // public variables
        public string Status { get; }
        public string Body { get; }
        public string ContentType { get; set; } = "application/json";

        // constructor
        public HttpResponse(string status, string body)
        {
            Status = status;
            Body = body;
        }
        public HttpResponse(string status)
        {
            Status = status;
            Body = "";
        }

        // public methods
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("HTTP/1.1 " + Status);

            if (Body != "")
            {
                builder.AppendLine($"Content-Type: {ContentType}");
                builder.AppendLine($"Content-Length: {Body.Length}");
            }

            builder.AppendLine();

            if (Body != "")
            {
                builder.AppendLine(Body);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
