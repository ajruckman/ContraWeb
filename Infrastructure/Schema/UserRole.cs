using System.Collections.Generic;
using FS3;
using Superset.Common;

namespace Infrastructure.Schema
{
    public static class UserRole
    {
        public static Roles NameToUserRole(string roleName) =>
            roleName.ToLower() switch
            {
                "restricted"    => Roles.Restricted,
                "privileged"    => Roles.Privileged,
                "administrator" => Roles.Administrator,
                _               => Roles.Undefined
            };

        public enum Roles
        {
            Undefined,
            Restricted,
            Privileged,
            Administrator
        }

        private static List<IOption<string>>? _options;

        public static List<IOption<string>> Options()
        {
            if (_options == null)
            {
                _options = new List<IOption<string>>();
                
                _options.Add(new Option<string>
                {
                    ID           = "Privileged",
                    OptionText   = "Privileged - View global log, and manage personal rules",
                    SelectedText = "Privileged"
                });
                
                _options.Add(new Option<string>
                {
                    ID           = "Administrator",
                    OptionText   = "Administrator - Manage global rules, system settings, and users",
                    SelectedText = "Administrator"
                });
            }

            return _options;
        }
    }
}