using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPChat;

namespace ServerTests
{
    public class ServerTest
    {
        static Server server;
        public static void Main()
        {
            server = new Server(1200);
            server.AddUser("MrDoritos", "coolguy");
            server.Start();
        }
    }
}
