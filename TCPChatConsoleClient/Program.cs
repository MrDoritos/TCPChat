using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPChat;
using System.Threading;
using System.Net;

namespace TCPChatConsoleClient
{
    class Program
    {
        static Client client;
        static IDictionary<uint, Message> messages = new Dictionary<uint, Message>();

        static void Main(string[] args)
        {
            client = new Client();
            client.Start(HostnamePrompt().ToString());
            //client.Start(new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 1200));
            LoginPrompt();
            var back = client.SendMessage(new RequestMessageRequest(0).Serialize());
            UpdateMessageList(back.ResponseMessages);
            new Thread(MessageReciever).Start();
            new Thread(HIDThread).Start();
            Thread.Sleep(-1);
        }

        static void HIDThread()
        {
            string buffer = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (buffer.Length > 0) buffer = buffer.Substring(0, buffer.Length - 1);
                        WriteCharacters(buffer);
                        break;
                    case ConsoleKey.Enter:
                        if (buffer.Length > 0)
                        {
                            ClearCharacters(); client.SendMessage(buffer); }
                        buffer = "";
                        break;
                    default:
                        if (buffer.Length < Console.WindowWidth - 1)
                        try { buffer += key.KeyChar;
                            WriteCharacters(buffer);
                        } catch { }
                        break;
                }
            }
        }

        static void WriteCharacters(string buffer)
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            Console.SetCursorPosition(0, bottom);
            Console.Write(buffer + " ");
            Console.SetCursorPosition(0, last);
        }

        static void ClearCharacters()
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            char[] erase = new char[Console.WindowWidth - 1];
            for (int i = 0; i < erase.Length; i++)
                erase[i] = ' ';
            Console.SetCursorPosition(0, bottom);
            Console.Write(erase, 0, erase.Length);
            Console.SetCursorPosition(0, last);
        }

        static void MessageReciever()
        {
            while (true)
            {
                var a = client.RecieveMessage();
                if (a.MessageType == TcpChatMessage.MessageTypes.ResponseMessagePacket)
                {
                    if (a.ResponseMessagesLength > 1)
                        UpdateMessageList(a.ResponseMessages);
                    else
                        PrintMessage(a.ResponseMessages[0]);
                }
            }
        }

        static void UpdateMessageList(Message[] newMessages)
        {
            foreach (var m in newMessages)
            {
                if (Exists(m.Id))
                {
                    messages[m.Id] = m;
                }
                else
                {
                    messages.Add(m.Id, m);
                }
            }
            ListMessages();
        }

        static bool Exists(uint id)
        {
            return messages.ContainsKey(id);
        }

        static void ListMessages()
        {
            var a = messages.OrderBy(n => n.Key);
            foreach (var m in a)
                PrintMessage(m.Value);
        }
        
        static void PrintMessage(Message m)
        {
            Console.WriteLine($"[{m.Author.Username} | {m.CreationTime.ToString("hh:mm")}]\r\n\t{m.Content}\a\r\n");
        }

        static IPAddress HostnamePrompt()
        {
            string hostname;
            while (true)
            {
                while (true)
                {
                    Console.Write("Hostname: ");
                    hostname = Console.ReadLine();
                    if (hostname.Length > 0) break; else Console.WriteLine("Invalid!");
                }
                try
                {
                    var a = Dns.GetHostAddresses(hostname);
                    if (a.Length > 1)
                        switch (ConsoleActions.MultipleChoice("Which address?", a))
                        {
                            case 0:
                                return a[0];
                            case 1:
                                return a[1];
                            case 2:
                                return a[2];
                            case 3:
                                return a[3];
                            case 4:
                                return a[4];
                        }
                    else if (a.Length > 0)
                    {
                        return a[0];
                    } else
                    {
                        Console.WriteLine("No records for this address!");
                    }
                }
                catch
                {
                    Console.WriteLine("No records for this address!");
                }                
            }
        }

        static void LoginPrompt()
        {
            switch (ConsoleActions.MultipleChoice("Log In", "Anonymous", "Existing User", "New user"))
            {
                case 0:
                    break;
                    //Existing user
                case 1:
                    while (true)
                    {
                        string username;
                        string password;
                        Console.Write("Username: ");
                        username = Console.ReadLine();
                        Console.Write("Password: ");
                        password = Console.ReadLine();
                        switch (client.LogIn(username, password))
                        {
                            case TcpChatMessage.MessageTypes.IncorrectCredentials:
                                Console.WriteLine("Incorrect username or password!");
                                break;
                            case TcpChatMessage.MessageTypes.UsernameExists:
                                Console.WriteLine("Unknown server response");
                                break;
                            case TcpChatMessage.MessageTypes.ResponseToken:
                                Console.WriteLine($"Logged in as {username}!");
                                return;
                        }
                    }
                    break;
                    //New user
                case 2:
                    while (true)
                    {
                        string username;
                        string password;
                        Console.Write("Username: ");
                        username = Console.ReadLine();
                        Console.Write("Password: ");
                        password = Console.ReadLine();
                        switch (client.Register(username, password))
                        {
                            case TcpChatMessage.MessageTypes.IncorrectCredentials:
                                Console.WriteLine("Unknown server response");
                                break;
                            case TcpChatMessage.MessageTypes.UsernameExists:
                                Console.WriteLine($"Username already exists!");
                                break;
                            case TcpChatMessage.MessageTypes.ResponseToken:
                                Console.WriteLine($"Logged in as {username}!"); 
                                return;
                        }
                    }
                    break;
            }
            
        }
    }
}
