using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using TCPChat;

namespace ClientTests
{
    public class Program
    {
        public static Client Client;
        public static void Main()
        {
            Client = new Client();
            int testnum = 1;
            IPAddress ip = null;
            while (ip == null)
            {
                try
                {
                    Console.Write("Hostname: ");
                    ip = Dns.GetHostAddresses(Console.ReadLine())[0];

                }catch(Exception e)
                {
                    ip = null;
                    Console.WriteLine(e.Message);
                }
            }
            Client.Start(new IPEndPoint(ip, 1200));
            while (true)
            {
                var a = Console.ReadKey();
                switch (a.Key)
                {
                    //Request all messages
                    case ConsoleKey.A:
                        Client.SendMessage(new RequestMessageRequest(0).Serialize());
                        break;
                        //Send message
                    case ConsoleKey.S:
                        Client.SendMessage($"test [{testnum}]");
                        testnum++;
                        break;
                    case ConsoleKey.L:
                        Client.SendMessage(new LoginMessage("MrDoritos", "coolguy"));
                        break;
                }
                
                //Client.SendMessage(new LoginMessage("bobtest", "someting wong"));
                //Client.SendMessage(new SendMessage("this is my token", "testing testing testing testing testing testing"));
            }
            //char[] token = new char[16] {'t','h','i','s',' ','i','s',' ','m','y',' ','t','o','k','e','n' };
            
            //TcpClient tcpClient = new TcpClient();
            //tcpClient.Connect(endPoint);
            //while (true)
            //{
            //    Console.ReadKey(true);           //1  2     3     4     5     6     7     8     9     10    11    12    13    14    15    16    17    20 21 22
            //    tcpClient.Client.Send(new byte[] { 1, 0x69, 0x61, 0x6e, 0x20, 0x69, 0x73, 0x20, 0x73, 0x75, 0x70, 0x65, 0x72, 0x20, 0x66, 0x6f, 0x6f, 0, 1, 1 });
            //    Console.WriteLine("Sent Message");
            //}
        }
    }
}
