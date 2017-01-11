using SimpleMessages.DB.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.DB.Models
{
    public class Message
    {
        public Guid? Guid { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        public DateTime? CreatedAt { get; set; }

        public MessageStatusType? Status { get; set; }

        [Required]
        [StringLength(20000, MinimumLength = 1)] // FB constraints
        public string Content { get; set; }

        public bool? IsDeleted { get; set; }

        public override string ToString()
        {
            return String.Format("[From: {0}; To: {1}, Content: {2}]", From, To, Content);
        }
    }
}
