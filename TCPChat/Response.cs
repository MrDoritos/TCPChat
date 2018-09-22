using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class Response
    {
        public TcpChatMessage.MessageTypes MessageType { get; private set; }
        
        public Response(TcpChatMessage.MessageTypes messageType) { MessageType = messageType; }

        public byte[] Serialize() { return new byte[] { (byte)MessageType }; }
    }
}
