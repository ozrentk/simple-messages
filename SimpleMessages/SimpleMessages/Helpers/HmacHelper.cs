using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Helpers
{
    internal static class HmacHelper
    {
        internal static string CreateBase64HashWithSecret(string password, out string base64secret)
        {
            var secret = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // fill random bytes
                rng.GetBytes(secret);

                // init hmac with new secret
                var hmac = new HMACMD5(secret);

                // get base64 secret
                base64secret = Convert.ToBase64String(secret);

                var bytesPassword = Encoding.UTF8.GetBytes(password);

                // do hash
                var hash = hmac.ComputeHash(bytesPassword);

                // return hash as base64
                return Convert.ToBase64String(hash);
            }
        }

        internal static string GetBase64HashFromPasswordAndSecret(string password, string base64secret)
        {
            // restore secret from base64
            var secret = Convert.FromBase64String(base64secret);

            // init hmac with new secret
            var hmac = new HMACMD5(secret);

            var bytesPassword = Encoding.UTF8.GetBytes(password);

            // do hash challenge pwd
            var hash = hmac.ComputeHash(bytesPassword);

            // return hash as base64
            return Convert.ToBase64String(hash);
        }

    }
}
