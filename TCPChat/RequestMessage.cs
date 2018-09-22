using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class RequestMessage
    {
        //The message goes a little like this
        //First octet = message Type
        //[2 to 17] 16 octets = token       OR       [2 to 3] 2 username Length
        //token                                   // [4 to 4 + username Length] username
        //2 octets content length  OR message id  // [4 + username Length + 1 to 4 + username Length + 2] 2 octets password Length
        //content                    //           // password

        public MessageTypes MessageType { get; private set; }
        public string Token { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public RequestMessage(MessageTypes messageType) { MessageType = messageType; }

        public enum MessageTypes
        {
            Unknown = 0,
            SendMesssage = 1,
            DeleteMessage = 2,
            Login = 3,
            RequestMessageList = 4,
            RequestMessage = 5,
            ResponseToken = 6,
            ResponseMessagePacket = 7,
            RegisterSelf = 10,
        }        

        public virtual byte[] Serialize()
        {
            byte[] baseBytes = new byte[] { (byte)MessageType };
            if (MessageType == MessageTypes.Login) { baseBytes = Server.Combine(baseBytes, LoginSerializer()); }
            else if (MessageType == MessageTypes.SendMesssage) { baseBytes = Server.Combine(baseBytes, MessageContentSerializer()); }
            return baseBytes;
        }

        private byte[] LoginSerializer()
        {
            byte[] username = Encoding.UTF8.GetBytes(Username);
            ushort usernameLength = (ushort)username.Length;
            byte[] password = Encoding.UTF8.GetBytes(Password);
            ushort passwordLength = (ushort)password.Length;
            return (Server.Combine(new byte[] { (byte)(usernameLength & 65280), (byte)(usernameLength & 255) }, username, new byte[] { (byte)(passwordLength & 65280), (byte)(passwordLength & 255) }, password));

        }

        private byte[] MessageContentSerializer()
        {
            byte[] token = Encoding.UTF8.GetBytes(Token);
            byte[] content = Encoding.UTF8.GetBytes(Content);
            ushort contentLength = (ushort)content.Length;
            return (Server.Combine(token, new byte[] { (byte)(contentLength & 65280),(byte)(contentLength & 255) }, content));
        }
    }
}
