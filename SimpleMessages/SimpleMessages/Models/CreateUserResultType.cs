using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Models
{
    public enum CreateUserResultType : int
    {
        Ok = 0,
        Error = 1,
        DuplicateUser = 2
    }
}
