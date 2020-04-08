using System;
using System.Collections.Generic;
using FS3;
using Superset.Common;

namespace Infrastructure.Schema
{
    public static class UserRole
    {
        public static Roles NameToUserRole(string? roleName) =>
            (roleName?.ToLower() ?? "") switch
            {
                "restricted"    => Roles.Restricted,
                "privileged"    => Roles.Privileged,
                "administrator" => Roles.Administrator,
                _               => Roles.Undefined
            };

        public static string RoleToDatabaseName(Roles role)
        {
            return role switch
            {
                Roles.Restricted    => "restricted",
                Roles.Privileged    => "privileged",
                Roles.Administrator => "administrator",
                Roles.Undefined     => throw new Exception(),
                _                   => throw new Exception(),
            };
        }

        public enum Roles
        {
            Undefined,
            Restricted,
            Privileged,
            Administrator
        }

        public static IEnumerable<IOption<string>> Options(Roles? selected = null)
        {
            var options = new List<IOption<string>>
            {
                new Option<string>
                {
                    ID           = "Privileged",
                    OptionText   = "Privileged - View global log, and manage personal rules",
                    SelectedText = "Privileged",
                    Selected     = selected?.ToString() == "Privileged"
                },
                new Option<string>
                {
                    ID           = "Administrator",
                    OptionText   = "Administrator - Manage global rules, system settings, and users",
                    SelectedText = "Administrator",
                    Selected     = selected?.ToString() == "Administrator"
                }
            };

            return options;
        }
    }
}