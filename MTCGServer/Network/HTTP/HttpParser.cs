using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Network.HTTP
{
    internal static class HttpParser
    {
        public static HttpRequest ParseHttpRequest(StreamReader reader)
        {
            string? curLine;
            int contentLength = 0;
            string token = "";

            // interpret request line
            if ((curLine = reader.ReadLine()) == null)
                throw new EndOfStreamException("Requester unexpectedly closed connection");

            string[] requestLine = curLine.Split(' ');

            if (requestLine.Length != 3)
                throw new HttpRequestException("Invalid HTTP request line");

            // check HTTP version
            if (requestLine[2] != "HTTP/1.1")
                throw new HttpRequestException("Invalid HTTP version");

            // get HTTP method
            HttpMethods method = requestLine[0] switch
            {
                "GET" => HttpMethods.GET,
                "POST" => HttpMethods.POST,
                "PUT" => HttpMethods.PUT,
                "DELETE" => HttpMethods.DELETE,
                _ => throw new HttpRequestException("Invalid HTTP method")
            };

            // get resource
            requestLine[1] = requestLine[1].Replace("%20", " ");
            string[] resource = requestLine[1].Split('/', StringSplitOptions.RemoveEmptyEntries);

            while ((curLine = reader.ReadLine()) != null)
            {
                // interpret headers

                // Content-Length
                if (curLine.Contains("Content-Length"))
                {
                    string[] contentLengthHeader = curLine.Split(':');

                    if (contentLengthHeader.Length != 2 || !int.TryParse(contentLengthHeader[1].Trim(), out contentLength))
                        throw new HttpRequestException("Invalid HTTP header, Content-Length");
                }

                // Authorization
                if (curLine.Contains("Authorization"))
                {
                    string[] authHeader = curLine.Split(':');

                    if (authHeader.Length != 2)
                        throw new HttpRequestException("Invalid HTTP header, Authorization");

                    token = authHeader[1].Trim();
                }

                // interpret body
                if (curLine == "" && contentLength > 0)
                {
                    char[] bodyBuffer = new char[contentLength];
                    reader.Read(bodyBuffer, 0, contentLength);
                    string body = new(bodyBuffer);

                    return new HttpRequest(method, resource, token, body);
                }
                else if (curLine == "" && contentLength == 0)
                {
                    return new HttpRequest(method, resource, token);
                }
            }

            throw new EndOfStreamException("Requester unexpectedly closed connection");
        }
    }
}
