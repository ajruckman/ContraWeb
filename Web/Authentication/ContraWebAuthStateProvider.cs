using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components.Authorization;

#pragma warning disable 1998

namespace Web.Authentication
{
    public class ContraWebAuthStateProvider : AuthenticationStateProvider
    {
        internal bool    IsAuthenticated    { get; private set; }
        internal User?   User               { get; private set; }
        internal string? Token              { get; private set; }
        internal string? AuthenticatedByMAC { get; private set; }

        private readonly ILocalStorageService _localStorage;

        public ContraWebAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task Initialize(string? ip)
        {
            Console.WriteLine(new string('-', 50));

            IsAuthenticated = false;

            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
                await AuthenticateByToken(token);
            else if (ip != null)
                await AuthenticateByIP(ip);
        }

        private async Task AuthenticateByToken(string token)
        {
            UserSession? userSession = await UserController.FindSession(token);
            if (userSession == null)
                return;

            User = await UserModel.Find(userSession.Username);
            if (User == null)
                return;

            Token = token;

            if (DateTime.Now.Subtract(userSession.RefreshedAt) > TimeSpan.FromDays(7))
                return;

            IsAuthenticated = true;
            Console.WriteLine(true);
            await UserController.RefreshSession(User, token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task AuthenticateByIP(string ip)
        {
            bool valid = IPAddress.TryParse(ip, out IPAddress address);
            if (!valid)
                return;

            List<Lease> matches = LeaseModel.FindByIP(address);
            if (matches.Count == 0)
                return;

            foreach (Lease match in matches)
            {
                List<User> users = await UserModel.FindByMAC(match.MAC);

                foreach (User user in users)
                {
                    Console.WriteLine($"{match.IP} / {match.MAC} -> {user.Username}");
                    User            = user;
                    IsAuthenticated = true;
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
            }
        }

        public async Task LogIn(string token)
        {
            IsAuthenticated = false;
            await _localStorage.SetItemAsync("token", token);
            await AuthenticateByToken(token);
        }

        public async Task LogOut()
        {
            IsAuthenticated = false;

            if (User != null)
            {
                await UserController.DeleteUserSession(User, Token!);
                User  = null;
                Token = null;
            }

            await _localStorage.RemoveItemAsync("token");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity = IsAuthenticated && User != null
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