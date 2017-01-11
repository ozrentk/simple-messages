using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Svc.Service
{
    public class MessageServiceAuthorizationPolicy : IAuthorizationPolicy
    {
        string _id;

        public MessageServiceAuthorizationPolicy()
        {
            this._id = Guid.NewGuid().ToString();
        }

        public string Id
        {
            get
            {
                return this._id;
            }
        }

        public ClaimSet Issuer
        {
            get
            {
                return ClaimSet.System;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            object identsObject;
            IList<IIdentity> idents;

            if (evaluationContext.Properties.TryGetValue("Identities", out identsObject) &&
                (idents = identsObject as IList<IIdentity>) != null)
            {
                foreach (IIdentity ident in idents)
                {
                    if (ident.IsAuthenticated && ident.AuthenticationType.Equals("CustomUserNameValidator"))
                    {
                        IIdentity customIdentity = new GenericIdentity(ident.Name/*, "myCustomAuthenticationType"*/);
                        IPrincipal customPrincipal = new GenericPrincipal(customIdentity, new[] { "user", "poweruser" });
                        evaluationContext.Properties["Principal"] = customPrincipal; // ident.Name;
                        return true;
                    }
                }
            }
            if (!evaluationContext.Properties.ContainsKey("Principal"))
            {
                evaluationContext.Properties["Principal"] = "";
            }
            return false;
        }
    }
}
