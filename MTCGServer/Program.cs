using MTCGServer.Network;
using System.Net;

namespace MTCGServer
{
    internal class Program
    {
        static void Main()
        {
            Server server = new(IPAddress.Loopback, 54321);

            server.Start();
        }
    }
}