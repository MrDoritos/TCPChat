using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class RequestMessageListRequest : RequestMessage
    {
        public UInt32[] RequestIds { get; }
        public RequestMessageListRequest(params UInt32[] requestids) : base(MessageTypes.RequestMessageList) { RequestIds = requestids; }

        new public byte[] Serialize()
        {
            byte[] baseBytes = base.Serialize();
            baseBytes = Server.Combine(baseBytes, GetBytes((UInt32)RequestIds.Length));
            foreach (var req in RequestIds)
                baseBytes = Server.Combine(baseBytes, GetBytes(req));
            return baseBytes;
        }

        public byte[] GetBytes(UInt32 id)
        {
            return new byte[] { (byte)(id & 4278190080), (byte)(id & 16711680), (byte)(id & 65280), (byte)(id & 255) };
        }
    }
}
