using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class User
    {
        public static User Anonymous = new User() { Username = "Anonymous", Password = "" };
        private User() { }
        public User(User user) { Username = user.Username; Password = user.Password; Token = user.Token; }
        public User(string username, string password) { Username = username; Password = password; Token = ""; }
        public User(string username) { Username = username; Password = ""; Token = ""; }

        public string Username { get; private set; }
        public string Password { get; private set; }
        //The 16 character token
        public string Token { get; private set; } = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
        public DateTime CreationTime { get; } = DateTime.Now;

        /// <summary>
        /// I'm not sorry for this method
        /// </summary>
        /// <returns>New token</returns>
        public string GenerateToken()
        {
            return (Token = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Take(16).ToArray()));
        }

        public override string ToString()
        {
            return $"{Username}";
        }
    }
}
