using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.DB.Models
{
    public class Notification
    {
        public Guid? Guid { get; set; }

        public DateTime? CreatedAt { get; set; }

        public NotificationType NotificationType { get; set; }

        public string OriginUser { get; set; }

        public string ForUser { get; set; }

        [Required]
        public Guid MessageGuid { get; set; }

        [Required]
        public MessageStatusType MessageStatus { get; set; }

        public string MessageContent { get; set; }

        public override string ToString()
        {
            return String.Format("[MessageGuid: {0}]", MessageGuid);
        }

    }
}
