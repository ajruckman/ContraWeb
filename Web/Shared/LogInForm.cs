#pragma warning disable 649

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;
using Superset.Web.Utilities;
using Superset.Web.Validation;
using Web.Authentication;

namespace Web.Shared
{
    public partial class LogInForm
    {
        private string? _username;
        private string? _password;
        private User?   _user;
        private bool?   _correct;

        private Validator<ValidationResult>? _validator;

        private ElementReference UsernameInput { get; set; }

        [Parameter]
        public bool Redirect { get; set; }
        
        protected override async Task OnInitializedAsync()
        {
            _validator = new Validator<ValidationResult>(
                () =>
                {
                    return new[]
                    {
                        _correct != null 
                            ? _correct.Value
                                ? new Validation<ValidationResult>(ValidationResult.Valid,   "Logged in")
                                : new Validation<ValidationResult>(ValidationResult.Invalid, "Username or password is incorrect")
                            : new Validation<ValidationResult>(ValidationResult.Undefined, "")
                    };
                }
            );
            
            _validator.Validate();

            await Register();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            await Utilities.FocusElement(JSRuntime, UsernameInput);
        }

        private async Task LogIn()
        {
            if (!CanLogIn())
                return;

            User? match = await UserController.LogIn(_username!, _password!);
            Console.WriteLine(match);

            if (match != null)
            {
                string token = await UserController.CreateUserSession(match);

                await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogIn(token);

                if (Redirect)
                    NavigationManager.NavigateTo("/");

                _correct = true;
            }
            else
            {
                _correct = false;
            }

            _validator!.Validate();
        }

        private bool CanLogIn()
        {
            return !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password);
        }

        private async Task Register()
        {
            try
            {
                _user = await UserModel.Find("admin") ??
                        UserController.Create("admin", "password", UserRole.Roles.Administrator, new List<PhysicalAddress>());
                Console.WriteLine(_user.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}