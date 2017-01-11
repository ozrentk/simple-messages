using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SimpleMessages.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Identity
{
    public class UserStore : 
        IUserStore<User, Guid>, 
        IUserPasswordStore<User, Guid>, 
        IUserLockoutStore<User, Guid>, 
        IUserTwoFactorStore<User, Guid>,
        IUserRoleStore<User, Guid>,
        IUserClaimStore<User, Guid>,
        IUserLoginStore<User, Guid>
    {
        private readonly Database _database;

        public UserStore()
        {
            var connStr = ConfigurationManager.ConnectionStrings["MessagesDb"].ConnectionString;
            _database = new Database(connStr);
        }

        #region USER STORE

        public System.Threading.Tasks.Task CreateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var newUser = _database.ExecuteAndGetRow<User>(
                "AddUser",
                new Dictionary<string, object> {
                    { "Guid", user.Id },
                    { "UserName", user.UserName },
                    { "Hash", user.PasswordHash }
                },
                (columns) => new User {
                    Id = Guid.Parse(columns["Guid"].ToString()),
                    UserName = columns["Name"].ToString(),
                    PasswordHash = columns["Hash"].ToString()
                });

            foreach (var role in user.Roles)
            {
                _database.Execute(
                    "AddUserToRole",
                    new Dictionary<string, object> {
                        { "UserName", user.UserName },
                        { "RoleName", role }
                    });
            }

            return Task.FromResult(newUser);
        }

        public System.Threading.Tasks.Task DeleteAsync(User user)
        {
            throw new NotImplementedException();
        }

        public User FindInternalAsync(Guid? userId = null, string userName = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));

            var user = _database.ExecuteAndGetRow<User>(
                "GetUser",
                new Dictionary<string, object> {
                    { "Guid", userId },
                    { "UserName", userName },
                },
                (columns) => new User
                {
                    Id = Guid.Parse(columns["Guid"].ToString()),
                    UserName = columns["Name"].ToString(),
                    PasswordHash = columns["Hash"].ToString()
                });

            if (user == null)
                return user;

            var roles = _database.ExecuteAndGetRows<string>(
                "GetUserRoles",
                new Dictionary<string, object> {
                    { "userName", user.UserName },
                },
                (columns) => columns["Name"].ToString());

            foreach (var role in roles)
                user.Roles.Add(role);

            return user;
        }


        public System.Threading.Tasks.Task<User> FindByIdAsync(Guid userId)
        {
            var user = FindInternalAsync(userId: userId);
            return Task.FromResult(user);
        }

        public System.Threading.Tasks.Task<User> FindByNameAsync(string userName)
        {
            var user = FindInternalAsync(userName: userName);
            return Task.FromResult(user);
        }

        public System.Threading.Tasks.Task UpdateAsync(User user)
        {
            throw new NotImplementedException();

            //return Task.FromResult(this.UserDb.Update(user));
        }

        public void Dispose()
        {
            //this.UserDb.FlushToDisk();
        }

        #endregion

        #region PASSWORD STORE

        public System.Threading.Tasks.Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public System.Threading.Tasks.Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(true);
        }

        public System.Threading.Tasks.Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            return Task.FromResult(0);
        }

        #endregion

        #region LOCKOUT STORE

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            //return Task.FromResult(user.LockoutEnabled);
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            //if (user == null)
            //    throw new ArgumentNullException(nameof(user));

            //if (!user.LockoutEndDateUtc.HasValue)
            //    throw new InvalidOperationException("LockoutEndDate has no value."); 

            //return Task.FromResult(new DateTimeOffset(user.LockoutEndDateUtc.Value));
            return Task.FromResult(DateTimeOffset.MinValue);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region TWO FACTOR

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            //return Task.FromResult(user.TwoFactorEnabled);
            return Task.FromResult(false);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region USERS - ROLES STORE

        public Task AddToRoleAsync(User user, string roleName)
        {
            throw new NotImplementedException();

            //if (user == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //if (string.IsNullOrEmpty(roleName))
            //{
            //    throw new ArgumentNullException(nameof(roleName));
            //}

            //var roles = RoleDb.TryLoadData();
            //var role = roles.SingleOrDefault(f => f.Name == roleName);

            //if (role == null)
            //{
            //    throw new KeyNotFoundException("role");
            //}

            //if (role != null && user.Roles != null && !user.Roles.Contains(roleName, StringComparer.InvariantCultureIgnoreCase))
            //{
            //    user.Roles.Add(roleName);
            //}

            //return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult<IList<string>>(user.Roles);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            return Task.FromResult(user.Roles.Contains(roleName));
            //return Task.FromResult(user.Roles.Contains(roleName, StringComparer.InvariantCultureIgnoreCase));
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            throw new NotImplementedException();

            //if (user == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //user.Roles.Remove(roleName);

            //return Task.FromResult(0);
        }

        #endregion

        #region USERS - CLAIM STORE

        public Task AddClaimAsync(User user, Claim claim)
        {
            throw new NotImplementedException();

            //if (user == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //if (claim == null)
            //{
            //    throw new ArgumentNullException(nameof(claim));
            //}

            //if (user.Claims != null && user.Claims.Any(f=>f.Value == claim.Value))
            //{
            //    user.Claims.Add(new UserClaim(claim));
            //}

            //return Task.FromResult(0);
        }

        public Task<IList<Claim>> GetClaimsAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = user.Claims.Select(clm => new Claim(clm.Type, clm.Value)).ToList();

            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task RemoveClaimAsync(User user, Claim claim)
        {
            throw new NotImplementedException();

            //if (user == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //if (claim == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //user.Claims.Remove(new UserClaim(claim));

            //return Task.FromResult(0);
        }

        #endregion

        #region USER - LOGINS

        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            throw new NotImplementedException();

            //if (user == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}
            //if (login == null)
            //{
            //    throw new ArgumentNullException(nameof(user));
            //}

            //if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            //{
            //    user.Logins.Add(new UserLoginInfo(login.LoginProvider, login.ProviderKey));
            //    UserDb.Update(user);
            //}

            //return Task.FromResult(true);
        }

        public Task<User> FindAsync(UserLoginInfo login)
        {
            throw new NotImplementedException();

            //var loginId = GetLoginId(login);
            //var user = UserDb.TryLoadData().SingleOrDefault(f => f.Id == int.Parse(loginId));
            //return Task.FromResult(user);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
