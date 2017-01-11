using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleMessages.Identity
{

    public class RoleManager : RoleManager<Role, int>
    {
        public RoleManager(IRoleStore<Role, int> store): base(store)
        {
            this.RoleValidator = new RoleValidator<Role, int>(this);
        }
    }
}
