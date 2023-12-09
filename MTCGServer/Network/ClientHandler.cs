using MTCGServer.BusinessLogic;
using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Network
{
    internal class ClientHandler
    {
        // private variables
        private readonly MainLogic _logic;
        private readonly Socket _client;

        // constructor
        public ClientHandler(MainLogic logic, Socket client)
        {
            _logic = logic;
            _client = client;
        }

        // public methods
        public void HandleConnection()
        {
            // create reader and writer streams to access socket
            using NetworkStream stream = new(_client, ownsSocket: true);
            using StreamReader reader = new(stream, Encoding.ASCII, false, 8192, leaveOpen: true);
            using StreamWriter writer = new(stream, Encoding.ASCII, 8192, leaveOpen: true) { AutoFlush = true };

            try
            {
                // get HTTP request
                HttpRequest request = HttpParser.ParseHttpRequest(reader);

                // get HTTP response
                HttpResponse response = _logic.HandleRequest(request);

                // send HTTP response
                writer.Write(response);
            }
            catch (HttpRequestException)
            {
                // catch misformed HTTP request
                writer.Write(new HttpResponse("400 Bad Request"));
            }
            catch (EndOfStreamException e)
            {
                // catch premature connection close
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
