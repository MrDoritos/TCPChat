using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class SetTokenMessage : RequestMessage
    {
        public SetTokenMessage(string token) : base(MessageTypes.ResponseToken) { _token = token; }

        private string _token;
        new public string Token { get { if (_token.Length < 16) { return User.Anonymous.Token; } else { return _token; } } }

        new public byte[] Serialize()
        {            
            return (Server.Combine(base.Serialize(), Encoding.ASCII.GetBytes(Token)));
        }
    }
}
