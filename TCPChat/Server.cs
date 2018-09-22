using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCPChat
{
    public class Server : TcpListener
    {
        /// <summary>
        /// Creates a new instance of the server
        /// </summary>
        /// <param name="port">Bind port</param>
        public Server(int port) : base(port) { }
        private readonly List<User> _registeredUsers = new List<User>();
        private readonly List<Message> _messages = new List<Message>();
        private UInt32 NextId { get => (UInt32)(_messages.Count + 1); }
        private List<TcpClient> _clients;
        
        /// <summary>
        /// Returns a user that matches the token, does not generate new tokens or add new users
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public User GetUser(string token)
        {
            if (_registeredUsers.Count < 1)
            {
                return User.Anonymous;
            }
            else
            {
                return _registeredUsers.FirstOrDefault(n => n.Token == token) ?? User.Anonymous;
            }
        }

        /// <summary>
        /// Returns a user that matches the username and password, does not generate new tokens or add new users
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User GetUser(string username, string password)
        {
            if (_registeredUsers.Count < 1)
            {
                return User.Anonymous;
            }
            else
            {
                return _registeredUsers.FirstOrDefault(n => n.Username == username && n.Password == password);
            }
        }

        public bool Exists(string username) { return (_registeredUsers.Count > 0 && _registeredUsers.Any(n => n.Username == username)); }
        public bool Exists(string username, string password) { return (_registeredUsers.Count > 0 && _registeredUsers.Any(n => n.Username == username && n.Password == password)); }
        public bool TokenExists(string token) { return (_registeredUsers.Count > 0 && _registeredUsers.Any(n => n.Token == token)); }

        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        /// <returns>token</returns>
        public string AddUser(string username, string password)
        {
            var user = new User(username, password);
            _registeredUsers.Add(user);
            return user.GenerateToken();
        }

        public void AddMessage(Message message)
        {
            MessageUpdate messageUpdate = new MessageUpdate(message);
            _messages.Add(message);
            for (int i = 0; i < _clients.Count; i++)
            {
                _clients[i].Client.Send(messageUpdate.Serialize());
            }
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            base.Start();
            _clients = new List<TcpClient>();
            new Thread(_Listen).Start();
        }

        private void _Listen()
        {
            do
            {
                var client = AcceptTcpClient();
                _clients.Add(client);
                Console.WriteLine($"New client {client.Client.RemoteEndPoint.ToString()}");
                new Thread(() => TakeConnection(client)).Start();
            } while (base.Active);
        }

        public virtual void TakeConnection(TcpClient client)
        {
            var netstream = client.GetStream();
            byte[] recieved = new byte[client.ReceiveBufferSize];
            try
            {
                do
                {
                    client.Client.Receive(recieved, 0, client.ReceiveBufferSize, SocketFlags.None);
                    var a = TcpChatMessage.Parse(recieved);
                    switch (a.MessageType)
                    {
                        case TcpChatMessage.MessageTypes.SendMesssage:
                            Message message = new Message(NextId, GetUser(a.Token), a.Content);
                            AddMessage(message);
                            Console.WriteLine($"Recieved a SendMessage request from {client.Client.RemoteEndPoint.ToString()}\r\nToken: \"{a.Token}\"\r\nContent: \"{a.Content}\"");
                            break;
                        case TcpChatMessage.MessageTypes.RegisterSelf:
                        case TcpChatMessage.MessageTypes.Login:
                            Console.WriteLine($"Recieved a {a.MessageType} request from {client.Client.RemoteEndPoint.ToString()}\r\nUsername: \"{a.Username}\"\r\nPassword: \"{a.Password}\"");
                            
                            if (a.MessageType == TcpChatMessage.MessageTypes.RegisterSelf)
                            {
                                if (Exists(a.Username))
                                {
                                    Console.WriteLine($"User already exists");
                                    client.Client.Send(new Response(TcpChatMessage.MessageTypes.UsernameExists).Serialize());
                                }
                                else
                                {
                                    Console.WriteLine($"Registered \"{a.Username}\"");
                                    client.Client.Send(new SetTokenMessage(AddUser(a.Username, a.Password)).Serialize());
                                }
                            }
                            else if (Exists(a.Username, a.Password))
                            {
                                var user = GetUser(a.Username, a.Password);
                                user.GenerateToken();
                                Console.WriteLine($"Successfully logged {user.Username}:{user.Password} in with session token {user.Token}");
                                client.Client.Send(new SetTokenMessage(user.Token).Serialize());
                            }
                            else
                            {
                                Console.WriteLine($"Invalid login attempt from {client.Client.RemoteEndPoint.ToString()}");
                                client.Client.Send(new Response(TcpChatMessage.MessageTypes.IncorrectCredentials).Serialize());
                            }
                            //var user = GetUser(a.Username, a.Password);
                            break;
                        case TcpChatMessage.MessageTypes.RequestMessageList:
                            break;
                        case TcpChatMessage.MessageTypes.RequestMessage:
                            if (a.RequestIds[0] == 0)
                            {
                                client.Client.Send(new MessageUpdate(_messages).Serialize());
                            }
                            break;
                        default:
                            Console.WriteLine($"Recieved {a.MessageType} from {client.Client.RemoteEndPoint}");
                            break;
                    }
                } while (client.Connected);
            } catch(Exception e)
            {
                Console.WriteLine($"Client {client.Client.RemoteEndPoint.ToString()} threw an exception and must reconnect!");
            }
            finally
            {
                //GC.SuppressFinalize(buffer);
                GC.SuppressFinalize(recieved);
                _clients.Remove(client);
                if (client.Connected) { client.Close(); }
            }
        }

        static public byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
