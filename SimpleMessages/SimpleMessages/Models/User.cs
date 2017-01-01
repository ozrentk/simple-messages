using SimpleMessages.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleMessages.Models
{
    public class User
    {
        public Guid Guid { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 6)]
        [ExistingUsername(isExisting: false, ErrorMessage = "This username is already taken, choose another one.")]
        public string UserName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
