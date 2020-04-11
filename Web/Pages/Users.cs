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
using Superset.Utilities;
using Web.Authentication;

namespace Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public partial class Users
    {
        private User? _user;

        private FlareTable<User>? _userTable;

        private string             _newUsername = "";
        Debouncer<string>?         _newUsernameDebouncer;
        private string             _newPassword = "";
        private Debouncer<string>? _newPasswordDebouncer;
        private UserRole.Roles     _newRole          = UserRole.Roles.Undefined;
        private Validator          _newUserValidator = new Validator();

        private string         _editedPassword    = "";
        private UserRole.Roles _editedRole        = UserRole.Roles.Undefined;
        private Validator      _editUserValidator = new Validator();

        private User?       _editing;
        private List<User>? _users;

        private FlareSelector<string>? _newRoleSelector;
        private FlareSelector<string>? _editedRoleSelector;

        protected override void OnInitialized()
        {
            LoadUsers();

            _user = ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).User;
            if (_user == null)
                throw new Exception("User is not authenticated; access to this page should not have been permitted.");

            _userTable = new FlareTable<User>(
                () => _users ?? new List<User>());

            _userTable.RegisterColumn(nameof(User.Username));
            _userTable.RegisterColumn(nameof(User.Role));

            _userTable.RegisterColumn("_Edit",   sortable: false, filterable: false, displayName: "", width: "56px");
            _userTable.RegisterColumn("_Remove", sortable: false, filterable: false, displayName: "", width: "82px");

            //

            _newUsernameDebouncer = new Debouncer<string>(value =>
            {
                Console.WriteLine("--");
                _newUsername = value;
                _newUserValidator.Refresh();
                InvokeAsync(StateHasChanged);
            }, _newUsername);
            
            _newPasswordDebouncer = new Debouncer<string>(value =>
            {
                _newPassword = value;
                _newUserValidator.Refresh();
                InvokeAsync(StateHasChanged);
            }, _newPassword);

            _newRoleSelector = new FlareSelector<string>(
                () => UserRole.Options(),
                false
            );

            _newRoleSelector.OnSelect += selected =>
            {
                _newRole = UserRole.NameToUserRole(selected.FirstOrDefault()?.ID);
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

                User? found = UserModel.Find(_newUsername).Result;
                if (found != null)
                    return new Validation(ValidationResult.Invalid, "Username is already registered");

                if (string.IsNullOrEmpty(_newPassword))
                    return new Validation(ValidationResult.Invalid, "Password is required");

                if (_newRole == UserRole.Roles.Undefined || _newRole == UserRole.Roles.Restricted)
                    return new Validation(ValidationResult.Invalid, "Role is invalid");

                return new Validation(ValidationResult.Valid, "User is valid");
            });

            _editUserValidator = new Validator(() =>
            {
                // if (string.IsNullOrEmpty(_editedPassword))
                // return new Validation(ValidationResult.Invalid, "Password is required");

                if (_editedRole == UserRole.Roles.Undefined || _editedRole == UserRole.Roles.Restricted)
                    return new Validation(ValidationResult.Invalid, "Role is invalid");

                return new Validation(ValidationResult.Valid, "Edits are valid");
            });
        }

        private void LoadUsers()
        {
            _users = UserModel.List();
        }

        private bool AllowSubmit()
        {
            return !string.IsNullOrEmpty(_newUsername) &&
                   !string.IsNullOrEmpty(_newPassword) &&
                   (_newRole == UserRole.Roles.Privileged || _newRole == UserRole.Roles.Administrator);
        }

        private void Add() => _editing = null;

        private void Edit(User user)
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
            if (_editing == null)
            {
                User newUser = UserController.Create(_newUsername, _newPassword, _newRole);

                _newUsername = "";
                _newPassword = "";
                _newRole     = UserRole.Roles.Undefined;

                LoadUsers();
                _userTable.InvalidateData();
                _newRoleSelector.InvalidateData(true);
            }
            else
            {
                if (_editedRole != UserRole.Roles.Undefined)
                {
                    _editing.Role = _editedRole;
                    await UserModel.UpdateRole(_editing);
                }

                if (_editedPassword != null)
                {
                    _editing.Password = _editedPassword;
                    await UserModel.UpdatePassword(_editing);
                }

                _editedRole     = UserRole.Roles.Undefined;
                _editedPassword = "";
                _editing        = null;

                LoadUsers();
                _userTable.InvalidateData();
                _newRoleSelector.InvalidateData(true);
            }

            Add();
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