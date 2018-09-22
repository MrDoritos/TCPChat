using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class RequestMessageRequest : RequestMessage
    {
        public RequestMessageRequest(UInt32 id) : base(MessageTypes.RequestMessage) { Id = id; }
        public UInt32 Id { get; }

        new public byte[] Serialize()
        {
            byte[] ahh = base.Serialize();
            return Server.Combine(ahh, new byte[] { (byte)(Id & 4278190080), (byte)(Id & 16711680), (byte)(Id & 65280), (byte)(Id & 255) });
        }
    }
}
