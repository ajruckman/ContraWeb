using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS3;
using FT3;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Web.Authentication;

namespace Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public partial class Users
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private User? _user;

        private FlareTable<User> _userTable;

        private string          _newUsername;
        private string          _newPassword;
        private string          _editedPassword;
        private UserRole.Roles? _newRole;
        private UserRole.Roles? _editedRole;
        private Validator       _newUserValidator  = new Validator();
        private Validator       _editUserValidator = new Validator();
        private List<User>?     _users;

        private User? _editing;

        private FlareSelector<string> _newRoleSelector;
        private FlareSelector<string> _editedRoleSelector;

        protected override void OnInitialized()
        {
            LoadUsers();

            _userTable = new FlareTable<User>(
                () => _users ?? new List<User>());

            _userTable.RegisterColumn(nameof(User.Username));
            _userTable.RegisterColumn(nameof(User.Role));

            _userTable.RegisterColumn("_Edit",   sortable: false, filterable: false, displayName: "", width: "57px");
            _userTable.RegisterColumn("_Remove", sortable: false, filterable: false, displayName: "", width: "83px");

            //

            _newRoleSelector = new FlareSelector<string>(
                () => UserRole.Options(),
                false
            );
            _newRoleSelector.OnSelect += selected =>
            {
                _newRole = UserRole.NameToUserRole(selected.First().ID);
                StateHasChanged();
            };

            _editedRoleSelector = new FlareSelector<string>(
                () => UserRole.Options(_editing?.Role),
                false,
                isDisabled: () => _editing.Username == _user.Username);
            _editedRoleSelector.OnSelect += selected =>
            {
                _editedRole = UserRole.NameToUserRole(selected.FirstOrDefault()?.ID);
                StateHasChanged();
            };

            //

            _newUserValidator = new Validator(() =>
            {
                if (string.IsNullOrEmpty(_newUsername))
                    return new Validation(ValidationResult.Invalid, "Username is required");

                if (string.IsNullOrEmpty(_newPassword))
                    return new Validation(ValidationResult.Invalid, "Password is required");

                if (_newRole == null || _newRole == UserRole.Roles.Undefined || _newRole == UserRole.Roles.Restricted)
                    return new Validation(ValidationResult.Invalid, "Role is invalid");

                return new Validation(ValidationResult.Valid, "User is valid");
            });

            _editUserValidator = new Validator(() =>
            {
                // if (string.IsNullOrEmpty(_editedPassword))
                // return new Validation(ValidationResult.Invalid, "Password is required");

                if (_editedRole == null || _editedRole == UserRole.Roles.Undefined || _editedRole == UserRole.Roles.Restricted)
                    return new Validation(ValidationResult.Invalid, "Role is invalid");

                return new Validation(ValidationResult.Valid, "Edits are valid");
            });
        }

        protected override async Task OnParametersSetAsync()
        {
            _user = ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).User;
            if (_user == null)
                throw new Exception("User is not authenticated; access to this page should not have been permitted.");
        }

        private void LoadUsers()
        {
            _users = UserModel.List();
        }

        private bool AllowSubmit()
        {
            return !string.IsNullOrEmpty(_newUsername) &&
                   !string.IsNullOrEmpty(_newPassword) &&
                   _newRole != null                    &&
                   (_newRole == UserRole.Roles.Privileged || _newRole == UserRole.Roles.Administrator);
        }

        private async Task Add() => _editing = null;

        private async Task Edit(User user)
        {
            _editing = user;
            _editedRoleSelector.InvalidateData();
        }

        private async Task Remove(User user)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Remove user '{user.Username}'?"});
            if (confirmed)
            {
                UserModel.Remove(user);
                LoadUsers();
                _userTable.InvalidateData();
            }
        }

        private async Task Submit()
        {
            User newUser = UserController.Create(_newUsername, _newPassword, _newRole.Value);
            await Add();
        }

        private bool CanRemoveUser(User user)
        {
            if (_editing != null)
                return false;

            if (user.Username == _user?.Username)
                return false;

            if (user.Role == UserRole.Roles.Administrator)
            {
                var admins = _users.Where(v => v.Role == UserRole.Roles.Administrator);
                if (admins.Count() == 1)
                    return false;
            }

            return true;
        }
    }
}