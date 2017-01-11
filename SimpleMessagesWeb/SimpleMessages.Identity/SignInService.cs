using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMessages.Identity
{
    public class SignInService: SignInManager<User, Guid>
    {
        public SignInService(UserManager userManager, IAuthenticationManager authenticationManager): base(userManager, authenticationManager)
        {

        }

        public override Task SignInAsync(User user, bool isPersistent, bool rememberBrowser)
        {
            return base.SignInAsync(user, isPersistent, rememberBrowser);
        }

        public override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            return base.PasswordSignInAsync(userName, password, isPersistent, shouldLockout);
        }
    }
}
