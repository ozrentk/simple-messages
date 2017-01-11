using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SimpleMessages.Web.Models.Authentication
{
    public class Login
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "User name length is between 6 and 256 characters")]
        [Display(Name ="User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password length is between 8 and 20 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}