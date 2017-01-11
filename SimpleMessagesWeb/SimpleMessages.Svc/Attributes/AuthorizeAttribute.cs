using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Svc.Attributes
{
    public class AuthorizeAttribute : Attribute
    {
        private string[] _roles;

        public AuthorizeAttribute(string Roles)
        {
            this._roles = Roles.Split(',');
        }

        public bool CheckRoles()
        {
            var token = WebOperationContext.Current.IncomingRequest.Headers["x-simplemessages-svctoken"];
            if (this._roles.Length > 0 && String.IsNullOrWhiteSpace(token))
                return false;

            var userId = Guid.Parse(token);

            var mgr = new Identity.UserManager(new Identity.UserStore());
            var user = mgr.FindByIdAsync(userId).Result;

            var matches = _roles.Intersect(user.Roles);

            return (matches.Count() > 0);
        }
    }
}
