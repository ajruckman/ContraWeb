using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Component;
using ColorSet.Components;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Subsegment.Bits;
using Subsegment.Constructs;
using Superset.Web.State;
using Web.Authentication;

namespace Web.Shared
{
    public partial class MainLayout
    {
        private readonly Configuration  _configuration = new Configuration();
        private readonly UpdateTrigger  _updateHeader  = new UpdateTrigger();
        private          ThemeLoader?   _themeLoader;
        private          string?        ClientIP { get; set; }
        private          UserRole.Roles _clientRole;

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        private readonly UpdateTrigger _onAuthChange = new UpdateTrigger();

        protected override void OnInitialized()
        {
            // ContraWebAuthStateProvider.AuthenticationStateChanged += _ => _onAuthChange.ReDiff();

            ClientIP = HttpContextAccessor.HttpContext.Connection?.RemoteIpAddress.ToString();

            _clientRole = Utility.GetRole(AuthenticationStateTask ?? throw new Exception("AuthenticationStateTask was not set"))
                                 .Result;

            _themeLoader            =  new ThemeLoader(LocalStorage, _configuration.ResourceManifests, "Dark");
            _themeLoader.OnComplete += StateHasChanged;

            ContraWebAuthStateProvider.AuthenticationStateChanged += _ => _updateHeader.Trigger();
        }

        protected override async Task OnInitializedAsync()
        {
            await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).Initialize(ClientIP);
        }

        private void GetClientRole(Task<AuthenticationState> authState)
        {
            _clientRole = Utility.GetRole(authState).Result;
        }

        private UserRole.Roles ClientRoleGetter() => _clientRole;

        public Header Header()
        {
            User? user = ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).User;

            Header? header = new Header(new List<IBit>
            {
                new Space(10),
                new Title("ContraWeb", "/"),
                new Filler(),
            });

            switch (user?.Role ?? UserRole.Roles.Undefined)
            {
                case UserRole.Roles.Administrator:
                    header.Add(new PageLink("Configure", "/configure"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    header.Add(new PageLink("Users", "/users"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    goto case UserRole.Roles.Privileged;

                case UserRole.Roles.Privileged:
                    header.Add(new PageLink("Whitelist", "/whitelist"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    header.Add(new PageLink("Blacklist", "/blacklist"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    header.Add(new PageLink("Generate", "/generate"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    goto case UserRole.Roles.Restricted;

                case UserRole.Roles.Restricted:

                    goto case UserRole.Roles.Undefined;

                case UserRole.Roles.Undefined:
                    header.Add(new PageLink("Stats", "/stats"));
                    header.Add(new Space());
                    header.Add(new Separator());
                    header.Add(new Space());

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (user == null)
            {
                header.Add(new Button("LOG IN", () => { NavigationManager.NavigateTo("/login"); }));
            }
            else
            {
                header.Add(new Chip(user.Username));
                header.Add(new Space());
                header.Add(new Button("LOG OUT",
                    async () => { await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogOut(); }));
            }

            header.Add(new Space(10));

            return header;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _themeLoader != null)
                await _themeLoader.Load();
        }

        private async Task LogOut()
        {
            await ((ContraWebAuthStateProvider) ContraWebAuthStateProvider).LogOut();
        }
    }
}