using System;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;
using Superset.Web.Utilities;
using Web.Authentication;

namespace Web.Shared
{
    public partial class LogInForm
    {
        private string? _username;
        private string? _password;
        private User?   _user;

        private ElementReference UsernameInput { get; set; }

        // protected override async Task OnInitializedAsync()
        // {
        // await Register();
        // }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            await Utilities.FocusElement(JSRuntime, UsernameInput);
        }

        private async Task<bool> LogIn()
        {
            if (!CanLogIn())
                return false;

            User? match = await UserController.LogIn(_username!, _password!);
            Console.WriteLine(match);

            if (match != null)
            {
                string token = await UserController.CreateUserSession(match);

                await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogIn(token);
            }

            NavigationManager.NavigateTo("/");

            return true;
        }

        private bool CanLogIn()
        {
            return !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password);
        }

        // private async Task Register()
        // {
        //     try
        //     {
        //         _user = await UserModel.Find("ajruckman") ??
        //                 UserController.Create("ajruckman", "123", UserRole.Roles.Administrator);
        //         Console.WriteLine(_user.Username);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return;
        //     }
        // }
    }
}