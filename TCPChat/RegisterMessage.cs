using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class RegisterMessage : RequestMessage
    {
        public RegisterMessage(string username, string password) : base(MessageTypes.RegisterSelf) { Username = username; Password = password; }

        public override byte[] Serialize()
        {
            return (Server.Combine(base.Serialize(), LoginSerializer()));
        }

        private byte[] LoginSerializer()
        {
            byte[] username = Encoding.UTF8.GetBytes(Username);
            ushort usernameLength = (ushort)username.Length;
            byte[] password = Encoding.UTF8.GetBytes(Password);
            ushort passwordLength = (ushort)password.Length;
            return (Server.Combine(new byte[] { (byte)(usernameLength & 65280), (byte)(usernameLength & 255) }, username, new byte[] { (byte)(passwordLength & 65280), (byte)(passwordLength & 255) }, password));
        }
    }
}
