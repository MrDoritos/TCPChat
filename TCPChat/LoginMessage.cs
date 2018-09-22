using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class LoginMessage : RequestMessage
    {
        public LoginMessage(string username, string password) : base(MessageTypes.Login) { Username = username; Password = password; }

        public override byte[] Serialize()
        {
            return base.Serialize();
        }
    }
}
