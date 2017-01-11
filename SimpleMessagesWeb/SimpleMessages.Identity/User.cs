using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SimpleMessages.Identity
{
    public class User: IUser<Guid>
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }

        //public bool LockoutEnabled { get; set; }
        //public DateTime? LockoutEndDateUtc { get; set; }
        //public bool TwoFactorEnabled { get; set; }

        public IList<string> Roles { get; private set; }
        public IList<UserClaim> Claims { get; private set; }
        public List<UserLoginInfo> Logins { get; private set; }

        public User()
        {
            this.Roles = new List<string>();
            this.Claims = new List<UserClaim>();
            this.Logins = new List<UserLoginInfo>();
        }

        public User(string userName)
            : this()
        {
            this.UserName = userName;
        }

        public User(Guid id, string userName): this()
        {
            this.Id = id;
            this.UserName = userName;
        }
    }
}
