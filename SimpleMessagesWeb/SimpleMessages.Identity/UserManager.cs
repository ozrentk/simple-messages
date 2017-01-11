﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMessages.Identity
{
    public class UserManager : UserManager<User, Guid>
    {
        public UserManager(IUserStore<User, Guid> store): base(store)
        {
            this.UserLockoutEnabledByDefault = false;
            // this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10);
            // this.MaxFailedAccessAttemptsBeforeLockout = 10;
            this.UserValidator = new UserValidator<User, Guid>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            // Configure validation logic for passwords
            this.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 4,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };
        }

        protected override Task<bool> VerifyPasswordAsync(IUserPasswordStore<User, Guid> store, User user, string password)
        {
            return base.VerifyPasswordAsync(store, user, password);
        }

    }
}
