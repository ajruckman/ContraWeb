using System;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Web.Authentication;

namespace Web.Shared
{
    public partial class Login
    {
        private User? _user;

        private string _username;
        private string _password;
        private string _loginMessage;

        protected override async Task OnInitializedAsync()
        {
            await Register();
        }

        private async Task LogIn()
        {
            User? match = await UserController.LogIn(_username, _password);
            Console.WriteLine(match);

            if (match != null)
            {
                string token = await UserController.CreateUserSession(match);
                
                await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogIn(token);
                
            }
            // Console.WriteLine(_username);
            // Console.WriteLine(_password);
            //
            // User? match = await UserController.LogIn(_username, _password);
            // Console.WriteLine(match);
            //
            // if (match != null)
            // {
            //     string token = await UserController.CreateUserSession(match);
            //     await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogIn(token);
            // }
        }

        private async Task LogOut()
        {
            await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogOut();
        }

        private async Task Register()
        {
            try
            {
                _user = await UserModel.Find("ajruckman") ?? UserController.Create("ajruckman", "123", UserRole.Roles.Administrator);
                Console.WriteLine(_user.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}