using Microsoft.AspNet.Identity;
using System;

namespace SimpleMessages.Identity
{
    public class Role: IRole<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
