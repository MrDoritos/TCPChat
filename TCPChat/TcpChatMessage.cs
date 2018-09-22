using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class TcpChatMessage
    {
        public TcpChatMessage(MessageTypes messageType) { MessageType = messageType; }
        public TcpChatMessage(TcpChatMessageStruct message)
        {
            MessageType = message.MessageType;
            Code = message.code;
            Content = message.content;
            ContentLength = message.contentLength;
            Token = message.token;
            Username = message.username;
            UsernameLength = message.usernameLength;
            Password = message.password;
            PasswordLength = message.passwordLength;
            ResponseMessagesLength = message.responseMessagesLength;
            ResponseMessages = message.responseMessages;
            RequestIds = message.requestIds;           
        }

        public MessageTypes MessageType { get; private set; }
        public TcpChatMessageStruct.Code Code { get; set; }
        public string Content { get; set; }
        public ushort ContentLength { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public ushort UsernameLength { get; set; }
        public string Password { get; set; }
        public ushort PasswordLength { get; set; }
        public UInt32[] RequestIds { get; set; }
        public ushort ResponseMessagesLength { get; set; }
        public Message[] ResponseMessages { get; set; }

        public static TcpChatMessage Parse(byte[] buffer)
        {
            //The message goes a little like this
            //First octet = message Type
            //[2 to 17] 16 octets = token       OR       [2 to 3] 2 username Length
            //token                                   // [4 to 4 + username Length] username
            //2 octets content length  OR message id  // [4 + username Length + 1 to 4 + username Length + 2] 2 octets password Length
            //content                    //           // password
            //
            //
            /*
             * A SendMessage request is like this:
             * [0](1 octet)SendMessage Declaration
             * [1 - 16](16 octets)Token
             * [17 - 18](2 octets)Content Length
             * [19 - END](Content length)Content
             * 
             * A Login request is like this:
             * [0](1 octet)Login Declaration
             * [1 - 2](2 octets)Username Length
             * [3 - ?](Username Length)Username
             * [? - ?](2 octets)Password Length
             * [? - END](Password length)Password
             * 
             * A MessagePackage request is like this
             * [0](1 octet)MessagePackage request declaration
             * (4 octets)Requested message id
             * 
             * A MessagePackage response is like this
             * [0](1 octet)MessagePackage response declaration
             * [1 - 2](2 octets)Number of response messages (used for deserialization, useless in practice)
             * --Begin Message--
             * (2 octets)Message byte length (used for deserialization)
             * (4 octets)Message Id
             * (2 octets)Message author's username length
             * (username length)Message author's username
             * (2 octets)Message content's length
             * (content length)Message content
             * (8 octets)Message timestamp in ticks
             * -- End  Message--
             * 
             * A MessagePackageList request is like this
             * [0](1 octet)declaration
             * [1 - 4](4 octets)Number of requests
             * *For each request*
             * (4 octets)Message Id
             */





            TcpChatMessageStruct message = new TcpChatMessageStruct();
            message.MessageType = (MessageTypes)buffer[0];
            switch (message.MessageType)
            {
                //May be unknown when an empty packet attempts to be parsed, aka the default value for the enumerable
                case MessageTypes.Unknown:
                    message.code = TcpChatMessageStruct.Code.ERROR;
                    break;
                case MessageTypes.SendMesssage:
                    message = GetToken(message, 1, buffer); // 1 is the first char of token
                    message = GetContent(message, 17, buffer);
                    break;
                case MessageTypes.Login:
                case MessageTypes.RegisterSelf:
                    message = GetUsernamePassword(message, 1, buffer);
                    break;
                case MessageTypes.ResponseMessagePacket:
                    message = GetPackagedMessages(message, 1, buffer);
                    break;
                case MessageTypes.RequestMessage:
                    message = GetMessageRequestMessage(message, 1, buffer);
                    break;
                case MessageTypes.RequestMessageList:
                    message = GetMessageRequestMessageList(message, 1, buffer);
                    break;
                case MessageTypes.ResponseToken:
                    message = GetToken(message, 1, buffer);
                    break;
            }
            return new TcpChatMessage(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position">Start of content</param>
        /// <param name="length">Amount of bytes in the content</param>
        private static TcpChatMessageStruct GetContent(TcpChatMessageStruct message, int position, int length, byte[] buffer)
        {
            message.content = (Encoding.UTF8.GetString(buffer.Skip(position).Take(length).ToArray()));
            return message;
        }

        private static TcpChatMessageStruct GetMessageRequestMessageList(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            UInt32 numOfRequests = (UInt32)((((buffer[position] << 8) | buffer[position + 1] << 8) | buffer[position + 2] << 8) | buffer[position + 3]);
            position += 4;
            List<UInt32> requests = new List<uint>();
            for (int i = 0; i < numOfRequests; i++)
            {
                requests.Add((UInt32)((((buffer[position] << 8) | buffer[position + 1] << 8) | buffer[position + 2] << 8) | buffer[position + 3]));
                position += 4;
            }
            message.requestIds = requests.ToArray();
            return message;
        }

        private static TcpChatMessageStruct GetMessageRequestMessage(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            UInt32 requestId = (UInt32)((((buffer[position] << 8) | buffer[position + 1] << 8) | buffer[position + 2] << 8) | buffer[position + 3]);
            message.requestIds = new UInt32[] { requestId };
            return message;
        }

        private static TcpChatMessageStruct GetPackagedMessages(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            ushort numOfMessages = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            message.responseMessagesLength = numOfMessages;
            position += 2;
            List<Message> messages = new List<Message>();
            for (int i = 0; i < numOfMessages; i++)
            {
                ushort messageLength = (ushort)((buffer[position] << 8) | buffer[position + 1]);
                position += 2;
                messages.Add(GetPackedMessage(position, buffer));
                position += messageLength;
            }
            message.responseMessages = messages.ToArray();
            return message;
        }

        private static Message GetPackedMessage(int position, byte[] buffer)
        {
            UInt32 messageId = (UInt32)((((buffer[position] << 8) | buffer[position + 1] << 8) | buffer[position + 2] << 8) | buffer[position + 3]);
            position += 4;
            ushort usernameLength = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            position += 2;
            string username = (Encoding.UTF8.GetString(buffer.Skip(position).Take(usernameLength).ToArray()));
            position += usernameLength;
            ushort contentLength = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            position += 2;
            string content = (Encoding.UTF8.GetString(buffer.Skip(position).Take(contentLength).ToArray()));
            position += contentLength;
            DateTime dateTime = new DateTime(BitConverter.ToInt64(buffer, position));
            position += 8;
            return new Message(dateTime, messageId, username, content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position">Position of content length</param>
        private static TcpChatMessageStruct GetContent(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            message.contentLength = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            message.content = (Encoding.UTF8.GetString(buffer.Skip(position + 2).Take(message.contentLength).ToArray()));
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position">Position of first char of token</param>
        private static TcpChatMessageStruct GetToken(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            message.token = (Encoding.ASCII.GetString(buffer.Skip(position).Take(16).ToArray()));
            return message;
        }

        /// <summary>
        /// Gets the username and password
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position">Position of username length</param>
        private static TcpChatMessageStruct GetUsernamePassword(TcpChatMessageStruct message, int position, byte[] buffer)
        {
            message.usernameLength = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            message.passwordLength = (ushort)((buffer[position + 2 + message.usernameLength] << 8) | buffer[position + 3 + message.usernameLength]);
            message.username = (Encoding.UTF8.GetString(buffer.Skip(position + 2).Take(message.usernameLength).ToArray()));
            message.password = (Encoding.UTF8.GetString(buffer.Skip(position + 4 + message.usernameLength).Take(message.passwordLength).ToArray()));
            return message;
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
            return (Server.Combine(token, new byte[] { (byte)(contentLength & 65280), (byte)(contentLength & 255) }, content));
        }

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
            IncorrectCredentials = 8,
            UsernameExists = 9,
            RegisterSelf = 10,
        }
    }

    public struct TcpChatMessageStruct
    {
        public enum Code
        {
            NOERROR = 0,
            ERROR = 1,
            FORBIDDEN = 2,
            LOGIN = 3,
            SHORTMESSAGE = 4,
            USERNAMEEXISTS = 5,
            CONTENTSHORT = 6,
        }
        public TcpChatMessage.MessageTypes MessageType;
        public Code code;
        public string content;
        public ushort contentLength;
        public string token;
        public string username;
        public ushort usernameLength;
        public string password;
        public ushort passwordLength;
        public ushort responseMessagesLength;
        public Message[] responseMessages;
        public UInt32[] requestIds;
    }
}
