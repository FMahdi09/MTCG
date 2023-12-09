using MTCGServer.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Network
{
    internal class Server
    {
        // private variables
        private readonly Socket _listeningSocket = new(AddressFamily.InterNetwork,
                                                        SocketType.Stream,
                                                        ProtocolType.Tcp);

        private readonly string _lastError = "no attempt to initialize Server";

        private readonly bool _listening = false;

        private readonly MainLogic _logic = new();

        // constructor
        public Server(IPAddress ip, int port)
        {
            try
            {
                // bind socket to ip and port                
                IPEndPoint ep = new(ip, port);
                _listeningSocket.Bind(ep);

                // start listening
                _listeningSocket.Listen(100);
                _listening = true;

                Console.WriteLine($"Started listening on: {ip}:{port}");
            }
            catch (Exception e)
            {
                _lastError = e.Message;
            }
        }

        // public functions
        public void Start()
        {
            // check if init of listeningsocket was successful
            if (!_listening)
            {
                Console.Error.WriteLine(_lastError);
                return;
            }

            // main server loop
            while (true)
            {
                // accept incoming connection
                Socket newClient = _listeningSocket.Accept();

                // start thread to handle connection
                ClientHandler clientHandler = new(_logic, newClient);
                Thread clientThread = new(() => clientHandler.HandleConnection());
                clientThread.Start();
            }
        }
    }
}
