using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Models
{
    public class AggregateNotification
    {
        public DateTime? CreatedAt { get; set; }

        [Required]
        public string ForUser { get; set; }

        [Required]
        public MessageStatusType MessageStatus { get; set; }

        public string MessageReceiver { get; set; }

        public override string ToString()
        {
            return String.Format("[Message source: {0}, Notification for: {1}, MessageStatus: {2}]", MessageReceiver, ForUser, MessageStatus);
        }

    }
}
