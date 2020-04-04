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
            Role = user.role switch
            {
                "restricted"    => UserRole.Restricted,
                "privileged"    => UserRole.Privileged,
                "administrator" => UserRole.Administrator,
                _               => UserRole.Undefined
            };
        }

        public User(string username, string salt, string hash, UserRole role)
        {
            Username = username;
            Salt     = salt;
            Password = hash;
            Role     = role;
        }

        public string   Username { get; set; }
        public string   Salt     { get; set; }
        public string   Password { get; set; }
        public UserRole Role     { get; set; }
    }

    public enum UserRole
    {
        Undefined,
        Restricted,
        Privileged,
        Administrator
    }
}