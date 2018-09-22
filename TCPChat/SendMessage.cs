using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class SendMessage : TcpChatMessage
    {
        public SendMessage(string token, string content) : base(MessageTypes.SendMesssage) { Token = token; Content = content; }

        public override byte[] Serialize()
        {
            return base.Serialize();
        }
    }
}
