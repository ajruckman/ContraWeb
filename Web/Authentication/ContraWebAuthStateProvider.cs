using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Authentication
{
    public class ContraWebAuthStateProvider : AuthenticationStateProvider
    {
        internal bool  IsAuthenticated { get; private set; }
        internal User? User            { get; private set; }

        private readonly ILocalStorageService _localStorage;

        public ContraWebAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task Initialize()
        {
            Console.WriteLine(new string('-', 50));

            IsAuthenticated = false;

            var token = await _localStorage.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token))
                return;

            UserSession? userSession = await UserController.FindSession(token);
            if (userSession == null)
                return;

            User = await UserModel.Find(userSession.Username);
            if (User == null)
                return;

            if (DateTime.Now.Subtract(userSession.RefreshedAt) > TimeSpan.FromDays(7))
                return;

            IsAuthenticated = true;
            Console.WriteLine(true);
            await UserController.RefreshSession(User);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LogIn(string token)
        {
            IsAuthenticated = false;
            await _localStorage.SetItemAsync("token", token);
            await Initialize();
        }

        public async Task LogOut()
        {
            IsAuthenticated = false;

            if (User != null)
            {
                await UserController.DeleteUserSession(User);
                User = null;
            }

            await _localStorage.RemoveItemAsync("token");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity = IsAuthenticated
                ? new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, User.Username),
                    new Claim(ClaimTypes.Role, User.Role.ToString())
                }, "ContraWebAuthStateProvider")
                : new ClaimsIdentity();

            AuthenticationState result = new AuthenticationState(new ClaimsPrincipal(identity));

            return result;
        }
    }
}