using Microsoft.AspNet.Identity;
using SimpleMessages.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMessages.Identity
{
    //public class RoleStorage
    //{
    //    public List<Role> Roles { get; set; } = new List<Role>();

    //    public List<Role> TryLoadData()
    //    {
    //        return Roles;
    //    }

    //    public void Add(Role role)
    //    {
    //        Roles.Add(role);
    //    }
    //}

    public class RoleStore : IRoleStore<Role, int>, IQueryableRoleStore<Role, int>
    {
        private readonly Database _database;
        
        //private readonly string FolderStorage = string.Empty;
        //private readonly RoleStorage RoleDb;

        //public RoleStore(string folderStorage)
        public RoleStore()
        {
            var connStr = ConfigurationManager.ConnectionStrings["MessagesDb"].ConnectionString;
            _database = new Database(connStr);

            //this.FolderStorage = folderStorage;
            //this.RoleDb = new RoleStorage();
            //this.RoleDb.Add(new Role { Id = 1, Name = "Administrators" });
        }

        public Task CreateAsync(Role role)
        {
            throw new NotImplementedException();

            //this.RoleDb.Add(role);
            //return Task.FromResult(role);
        }

        public Task DeleteAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public Task<Role> FindByIdAsync(int roleId)
        {
            throw new NotImplementedException();

            //Role role = null;
            //IList<Role> roles = this.RoleDb.TryLoadData();
            //if (roles == null || roles.Count == 0)
            //{
            //    return Task.FromResult(role);
            //}

            //role = roles.SingleOrDefault(f => f.Id == roleId);

            //return Task.FromResult(role);
        }

        public Task<Role> FindByNameAsync(string roleName)
        {
            throw new NotImplementedException();

            //Role role = null;
            //IList<Role> roles = this.RoleDb.TryLoadData();
            //if (roles == null || roles.Count == 0)
            //{
            //    return Task.FromResult(role);
            //}

            //role = roles.SingleOrDefault(f => f.Name == roleName);

            //return Task.FromResult(role);
        }

        public Task UpdateAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //this.RoleDb.FlushToDisk();
        }

        public IQueryable<Role> Roles
        {
            get
            {
                throw new NotImplementedException();

                //return (this.RoleDb.TryLoadData().AsQueryable<Role>());
            }
        }
    }
}
