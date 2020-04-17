using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Database.ContraWebDB;

namespace Infrastructure.Schema
{
    public class User
    {
        public User(user user)
        {
            Username = user.username;
            Salt     = user.salt;
            Password = user.password;
            Role     = UserRole.NameToUserRole(user.role);
            MACs     = user.macs.ToList();
        }

        public User(string username, string salt, string hash, UserRole.Roles role, List<PhysicalAddress> macs)
        {
            Username = username;
            Salt     = salt;
            Password = hash;
            Role     = role;
            MACs     = macs;
        }

        public string                Username { get; set; }
        public string                Salt     { get; set; }
        public string                Password { get; set; }
        public UserRole.Roles        Role     { get; set; }
        public List<PhysicalAddress> MACs     { get; set; }
    }
}