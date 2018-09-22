using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace TCPChat
{
    public class Client
    {
        public Func<TcpChatMessage, TcpChatMessage> Recieved;
        public Func<Message> MessageRecieved;
        public List<Message> Messages { get; private set; }

        private NetworkStream _netStream;

        public IPEndPoint Bind { get; private set; }
        //private NetworkStream _netStream;
        private TcpClient _client;
        private string _token = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
        //List<TcpChatMessage> recievedMessages = new List<TcpChatMessage>();

       //public Client(IPEndPoint endPoint)
       //{
       //     _client = new TcpClient();
       //     Bind = endPoint;
       //}

        public Client()
        {
            _client = new TcpClient();
            Messages = new List<Message>();
        }
        
        public void Start(IPEndPoint endPoint)
        {
            Bind = null;
            _client = new TcpClient();
            _client.Connect(endPoint);
            _netStream = _client.GetStream();
            //new Thread(() => TakeConnection(_client)).Start();
            Bind = endPoint;
        }

        public void Start(string hostname)
        {
            Bind = null;
            _client = new TcpClient(hostname, 1200);
        }

        private void InvokeLoginRelated(Delegate[] delegates, TcpChatMessage message)
        {
            var handleTasks = new Thread[delegates.Length];
            for (int a = 0; a < delegates.Length; a++)
                ((Func<TcpChatMessage, int>)delegates[a])(message);
        }

        public virtual void TakeConnection(TcpClient client)
        {
            var netstream = client.GetStream();
            byte[] recieved = new byte[client.ReceiveBufferSize];
            try
            {
                do
                {
                    client.Client.Receive(recieved, client.ReceiveBufferSize, SocketFlags.None);
                    var a = TcpChatMessage.Parse(recieved);
                    
                    var invokelist = Recieved.GetInvocationList();
                    if (invokelist.Length > 0)
                    InvokeAll(Recieved.GetInvocationList(), a);
                    //switch (a.MessageType)
                    //{
                    //    case TcpChatMessage.MessageTypes.ResponseMessagePacket:
                    //        foreach (var message in a.ResponseMessages)
                    //            Console.WriteLine($"[{message.Author.Username}] {message.Content}\r\n\r\n\t{message.CreationTime.ToString("hh:mm")}\r\n");
                    //        break;
                    //    case TcpChatMessage.MessageTypes.ResponseToken:
                    //        _token = a.Token;
                    //        break;
                    //}
                } while (client.Connected);
            } catch(Exception e)
            {
                Console.WriteLine($"Client died");
            }
            finally
            {
                GC.SuppressFinalize(recieved);
                if (client.Connected) { client.Close(); }
            }
        }
                
        private void InvokeAll(Delegate[] delegates, TcpChatMessage message)
        {
            var handleTasks = new Thread[delegates.Length];
            for (int a = 0; a < delegates.Length; a++)
                ((Func<TcpChatMessage, TcpChatMessage>)delegates[a])(message);
        }

        /// <summary>
        /// For now its a fire-and-forget, later it will be a blocking call for a server response
        /// </summary>
        public void SendMessage(LoginMessage loginMessage)
        {
            byte[] thing = loginMessage.Serialize();
            _client.Client.Send(loginMessage.Serialize());
        }

        public void SendMessage(SendMessage sendMessage)
        {
            byte[] thing = sendMessage.Serialize();
            _client.Client.Send(sendMessage.Serialize());
        }

        public void SendMessage(string message)
        {
            _client.Client.Send(new SendMessage(_token, message).Serialize());
        }

        public TcpChatMessage SendMessage(byte[] message)
        {
            var thread = new Thread(() => RecieveMessage());
            thread.Start();
            _client.Client.Send(message);
            while (thread.ThreadState == ThreadState.Running) { Thread.Sleep(10); }
            return returnMessage;
        }

        public void SendMessage(RequestMessage message)
        {
            _client.Client.Send(message.Serialize());
        }

        public TcpChatMessage.MessageTypes LogIn(string username, string password)
        {
            LoginMessage loginMessage = new LoginMessage(username ?? "", password ?? "");
            
            while (true)
            {
                var ddd = new Thread(() => RecieveMessage());
                ddd.Start();
                SendMessage(loginMessage);
                while (ddd.ThreadState == ThreadState.Running) { Thread.Sleep(10); }
                switch (returnMessage.MessageType)
                {
                    case TcpChatMessage.MessageTypes.ResponseToken:
                        _token = returnMessage.Token;
                        return TcpChatMessage.MessageTypes.ResponseToken;
                    case TcpChatMessage.MessageTypes.IncorrectCredentials:
                        return TcpChatMessage.MessageTypes.IncorrectCredentials;
                    case TcpChatMessage.MessageTypes.UsernameExists:
                        return TcpChatMessage.MessageTypes.UsernameExists;                        
                }
            }
        }

        public TcpChatMessage.MessageTypes Register(string username, string password)
        {
            RegisterMessage message = new RegisterMessage(username ?? "", password ?? "");

            while (true)
            {
                switch (SendMessage(message.Serialize()).MessageType)
                {
                    case TcpChatMessage.MessageTypes.ResponseToken:
                        _token = returnMessage.Token;
                        return TcpChatMessage.MessageTypes.ResponseToken;
                    case TcpChatMessage.MessageTypes.IncorrectCredentials:
                        return TcpChatMessage.MessageTypes.IncorrectCredentials;
                    case TcpChatMessage.MessageTypes.UsernameExists:
                        return TcpChatMessage.MessageTypes.UsernameExists;
                }
            }
        }
        
        public void CreateAccount(string username, string password) { }

        public TcpChatMessage returnMessage;

        public TcpChatMessage RecieveMessage()
        {            
            byte[] recieved = new byte[_client.Client.ReceiveBufferSize];
            _client.Client.Receive(recieved, _client.Client.ReceiveBufferSize, SocketFlags.None);
            returnMessage = TcpChatMessage.Parse(recieved);
            return returnMessage;
        }
        
        static public byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
