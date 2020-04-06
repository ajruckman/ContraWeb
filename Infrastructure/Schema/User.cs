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
        }

        public User(string username, string salt, string hash, UserRole.Roles role)
        {
            Username = username;
            Salt     = salt;
            Password = hash;
            Role     = role;
        }

        public string         Username { get; set; }
        public string         Salt     { get; set; }
        public string         Password { get; set; }
        public UserRole.Roles Role     { get; set; }
    }
}