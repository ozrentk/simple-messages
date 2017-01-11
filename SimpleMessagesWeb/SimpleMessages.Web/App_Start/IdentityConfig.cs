using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using SimpleMessages.Web.Models;

namespace SimpleMessages.Web.App_Start
{
    public class IdentityConfig
    {
        public void Configuration(IAppBuilder app)
        {
            // Custom implementation of user/role manager/store 
            app.CreatePerOwinContext<SimpleMessages.Identity.UserManager>(() => 
                new SimpleMessages.Identity.UserManager(
                    new SimpleMessages.Identity.UserStore()));

            app.CreatePerOwinContext<SimpleMessages.Identity.RoleManager>(() => 
                new SimpleMessages.Identity.RoleManager(
                    new SimpleMessages.Identity.RoleStore()));

            app.CreatePerOwinContext<SimpleMessages.Identity.SignInService>((options, context) => 
                new SimpleMessages.Identity.SignInService(
                    context.GetUserManager<SimpleMessages.Identity.UserManager>(), context.Authentication));

            // Configure the OWin sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<SimpleMessages.Identity.UserManager, SimpleMessages.Identity.User, Guid>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentityCallback: (manager, user) =>
                        {
                            var userIdentity = manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                            return (userIdentity);
                        },
                        getUserIdCallback: (id) => (Guid.Parse(id.GetUserId()))
                    )
                }
            });
        }
    }
}