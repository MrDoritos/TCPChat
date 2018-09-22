using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    class MessageUpdate : TcpChatMessage
    {
        private Message[] _messages;
        public MessageUpdate(params Message[] messages) : base(MessageTypes.ResponseMessagePacket) { _messages = messages; }
        public MessageUpdate(IEnumerable<Message> messages) : base(MessageTypes.ResponseMessagePacket) { _messages = messages.ToArray(); }
        public MessageUpdate(List<Message> messages): base(MessageTypes.ResponseMessagePacket) { _messages = messages.ToArray(); }

        new public byte[] Serialize()
        {
            return Server.Combine(base.Serialize(), SerializeMessages());
        }

        private byte[] SerializeMessage(Message message)
        {
            byte[] messageId = new byte[] { (byte)(message.Id & 4278190080), (byte)(message.Id & 16711680), (byte)(message.Id & 65280), (byte)(message.Id & 255) };
            byte[] content = Encoding.UTF8.GetBytes(message.Content);
            byte[] contentLength = new byte[] { (byte)(content.Length & 65280), (byte)(content.Length & 255) };
            byte[] username = Encoding.UTF8.GetBytes(message.Author.Username);
            byte[] usernameLength = new byte[] { (byte)(username.Length & 65280), (byte)(username.Length & 255) };
            byte[] date = BitConverter.GetBytes(message.CreationTime.Ticks);
            return Server.Combine(messageId, usernameLength, username, contentLength, content, date);
        }

        private byte[] SerializeMessages()
        {
            byte[] toreturn = new byte[2] { (byte)(_messages.Length & 65280), (byte)(_messages.Length & 255) };
            foreach (var message in _messages)
            {
                var serialized = SerializeMessage(message);
                toreturn = Server.Combine(toreturn, new byte[] { (byte)(serialized.Length & 65280), (byte)(serialized.Length & 255) }, serialized);
            }
            return toreturn;
        }
    }
}
