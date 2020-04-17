using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using API.Component;
using FlareSelect;
using FlareTables;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Subsegment.Bits;
using Subsegment.Constructs;
using Superset.Utilities;
using Superset.Web.Validation;
using Web.Authentication;
using Web.Components.EditableList;

namespace Web.Pages
{
    [Authorize(Roles = "Administrator")]
    public partial class Users
    {
        private User? _user;

        private FlareTable<User>? _userTable;

        private string                      _newUsername = "";
        Debouncer<string>?                  _newUsernameDebouncer;
        private string                      _newPassword = "";
        private Debouncer<string>?          _newPasswordDebouncer;
        private UserRole.Roles              _newRole          = UserRole.Roles.Undefined;
        private Validator<ValidationResult> _newUserValidator = new Validator<ValidationResult>();

        private string                      _editedPassword    = "";
        private UserRole.Roles              _editedRole        = UserRole.Roles.Undefined;
        private Validator<ValidationResult> _editUserValidator = new Validator<ValidationResult>();

        private User?       _editing;
        private List<User>? _users;

        private FlareSelector<string>? _newRoleSelector;
        private FlareSelector<string>? _editedRoleSelector;

        private EditableList? _newMACsSelector;
        private EditableList? _editMACsSelector;

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
                _newUserValidator.Validate();
                InvokeAsync(StateHasChanged);
            }, _newUsername);

            _newPasswordDebouncer = new Debouncer<string>(value =>
            {
                _newPassword = value;
                _newUserValidator.Validate();
                InvokeAsync(StateHasChanged);
            }, _newPassword);

            //

            _newRoleSelector = new FlareSelector<string>(
                () => UserRole.Options(),
                false
            );

            _newRoleSelector.OnSelect += selected =>
            {
                _newRole = UserRole.NameToUserRole(selected.FirstOrDefault()?.ID);
                _newUserValidator.Validate();
                InvokeAsync(StateHasChanged);
            };

            _editedRoleSelector = new FlareSelector<string>(
                () => UserRole.Options(_editing?.Role),
                false,
                isDisabled: () => _editing?.Username == _user.Username);

            _editedRoleSelector.OnSelect += selected =>
            {
                _editedRole = UserRole.NameToUserRole(selected.FirstOrDefault()?.ID);
                _editUserValidator.Validate();
                InvokeAsync(StateHasChanged);
            };

            //

            _newMACsSelector = new EditableList(
                validator: ValidateMAC,
                placeholder: "Add a MAC",
                transformer: Utility.FormatMAC
            );

            _newMACsSelector.OnUpdate += macs =>
                _user!.MACs = macs.Select(v => PhysicalAddress.Parse(Utility.CleanMAC(v))).ToList();

            _editMACsSelector = new EditableList(
                validator: ValidateMAC,
                placeholder: "Add a MAC",
                transformer: Utility.FormatMAC
            );

            _editMACsSelector.OnUpdate += macs =>
                _editing!.MACs = macs.Select(v => PhysicalAddress.Parse(Utility.CleanMAC(v))).ToList();

            //

            _newUserValidator = new Validator<ValidationResult>(() =>
            {
                if (string.IsNullOrEmpty(_newUsername))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Username is required")};

                User? found = UserModel.Find(_newUsername).Result;
                if (found != null)
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Username is already registered")};

                if (string.IsNullOrEmpty(_newPassword))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Password is required")};

                if (_newRole == UserRole.Roles.Undefined || _newRole == UserRole.Roles.Restricted)
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Role is invalid")};

                return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "User is valid")};
            });

            _editUserValidator = new Validator<ValidationResult>(() =>
            {
                // if (string.IsNullOrEmpty(_editedPassword))
                // return new Validation(ValidationResult.Invalid, "Password is required");

                if (_editedRole == UserRole.Roles.Undefined || _editedRole == UserRole.Roles.Restricted)
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Role is invalid")};

                return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "Edits are valid")};
            });

            _newUserValidator.Validate();
            
            //
            
            BuildHeaders();
        }

        private void LoadUsers()
        {
            _users = UserModel.List();
        }

        private bool AllowSubmit()
        {
            return _editing == null
                ? !_newUserValidator.AnyOfType(ValidationResult.Invalid, includeOverall: true)
                : !_editUserValidator.AnyOfType(ValidationResult.Invalid, includeOverall: true);
        }

        private void Add()
        {
            _editing = null;
            BuildHeaders();
        }

        private void Edit(User user)
        {
            _editing = user;
            _editedRoleSelector!.InvalidateData();
            _editMACsSelector!.Set(user.MACs.Select(Utility.FormatMAC).ToList());
            BuildHeaders();
        }

        private async Task Remove(User user)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Remove user '{user.Username}'?"});
            if (confirmed)
            {
                UserModel.Remove(user);
                LoadUsers();
                _userTable!.InvalidateData();
            }
        }

        private async Task Submit()
        {
            if (_editing == null)
            {
                User newUser = UserController.Create(_newUsername, _newPassword, _newRole, new List<PhysicalAddress>());

                _newUsername = "";
                _newPassword = "";
                _newRole     = UserRole.Roles.Undefined;

                LoadUsers();
                _userTable!.InvalidateData();
                _newRoleSelector!.InvalidateData(true);
                _newUserValidator.Validate();
            }
            else
            {
                if (_editedRole != UserRole.Roles.Undefined)
                {
                    _editing.Role = _editedRole;
                    await UserModel.Update(_editing);
                }

                if (_editedPassword != null)
                {
                    await UserController.UpdatePassword(_editing, _editedPassword);
                }

                _editedRole     = UserRole.Roles.Undefined;
                _editedPassword = "";
                _editing        = null;

                LoadUsers();
                _userTable!.InvalidateData();
                _newRoleSelector!.InvalidateData(true);
                _editUserValidator.Validate();
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

        public (ValidationResult, MarkupString) ValidateMAC(string s)
        {
            if (s.Length == 0)
                return (ValidationResult.Undefined, new MarkupString(""));

            return Utility.ValidateMAC(s)
                ? (ValidationResult.Valid, new MarkupString("Valid MAC"))
                : (ValidationResult.Invalid, new MarkupString("Invalid MAC"));
        }

        private Subheader _listSubheader;
        private Subheader _inputSubheader;

        private void BuildHeaders()
        {
            _listSubheader = new Subheader(
                new List<IBit>
                {
                    new Space(10),
                    new Title("Options")
                }
            );

            _inputSubheader = new Subheader(
                new List<IBit>
                {
                    new Space(10),
                    new Title(_editing == null ? "Add new user" : "Edit user"),
                }
            );
            
            if (_editing != null)
                _inputSubheader.Add(new Button("CANCEL", Add, Button.Color.Red));
        }
    }
}