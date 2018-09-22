using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    public class Message
    {
        public Message(UInt32 id, User author, string content) { Id = id; Content = content; Author = author; CreationTime = DateTime.Now; }
        public Message(DateTime creationTime, UInt32 id, User author, string content) { Id = id; Content = content; Author = author; CreationTime = creationTime; }
        public Message(DateTime creationTime, UInt32 id, string username, string content) { CreationTime = creationTime; Id = id; Content = content; Author = new User(username); }
        public UInt32 Id { get; }
        public string Content { get; }
        public User Author { get; }
        public DateTime CreationTime { get; }
    }
}
