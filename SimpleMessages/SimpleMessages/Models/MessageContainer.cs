using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Models
{
    public class MessageContainer
    {
        public Message[] Messages { get; set; }
        public Notification[] Notifications { get; set; }
    }
}
