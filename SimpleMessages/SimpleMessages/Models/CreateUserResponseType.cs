using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Models
{
    public enum CreateUserResponseType : byte
    {
        OK = 0,
        DuplicateUser = 1,
        Error = 2
    }
}
