using System;
using System.Reflection;

namespace Utopia.Server
{
    class Program
    {
        private static Server _server;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Utopia game server v{1} Protocol: v{0}", Server.ServerProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);
            
            _server = new Server();
            _server.Listen();
            
            while (true)
            {
                var command = Console.ReadLine().ToLower();

                switch (command)
                {
                    case "exit":
                        _server.Dispose();
                        return;
                    case "status":
                        break;
                }
            }
        }
    }
}
