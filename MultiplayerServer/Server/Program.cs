using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerController server = new ServerController();
            server.StartServer("127.0.0.1", 7777);
            server.RunServer();
        }
    }
}
