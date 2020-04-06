using FS3;
using FT3;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Authorization;

namespace Web.Pages
{
    [Authorize(Roles ="Administrator")]
    public partial class Users
    {
        private FlareTable<User> _userTable;

        private string         _newUsername;
        private string         _newPassword;
        private UserRole.Roles _newRole;

        private FlareSelector<string> _newRoleSelector;

        protected override void OnInitialized()
        {
            _userTable = new FlareTable<User>(
                UserModel.List
            );

            _userTable.RegisterColumn(nameof(User.Username));
            _userTable.RegisterColumn(nameof(User.Role));

            //

            _newRoleSelector = new FlareSelector<string>(
                UserRole.Options,
                false
            );
        }
    }
}