using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleMessages.DAL;

namespace SimpleMessages.Attributes
{
    internal sealed class ExistingUsernameAttribute : ValidationAttribute
    {
        private readonly Database _database;
        private readonly bool _isExisting;

        public ExistingUsernameAttribute(bool isExisting)
        {
            this._database = new Database();
            this._isExisting = isExisting;
        }

        public override bool IsValid(object value)
        {
            var strValue = value as string;

            return _database.CheckIfUsernameExists(strValue, this._isExisting);
        }
    }
}
