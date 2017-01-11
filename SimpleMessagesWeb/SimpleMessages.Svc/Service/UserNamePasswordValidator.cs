using SimpleMessages.Svc.Helpers;
using SimpleMessages.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Svc.Service
{
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        readonly Database _database;

        public CustomUserNameValidator()
        {
            this._database = new Database();
        }

        // Pass through annoying exceptions - comment out while debugging
        [DebuggerStepThrough]
        public override void Validate(string userName, string password)
        {
            string base64secret;
            var dbBase64hash = _database.GetHashForUsername(userName, out base64secret);
            if (dbBase64hash == null)
                throw new FaultException("Unknown username or incorrect password");

            // return hash as base64
            var base64hash = HmacHelper.GetBase64HashFromPasswordAndSecret(password, base64secret);

            if (!dbBase64hash.Equals(base64hash))
                throw new FaultException("Unknown username or incorrect password");
        }
    }

}
