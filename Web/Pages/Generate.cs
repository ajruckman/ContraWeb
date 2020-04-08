
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Superset.Web.State;

namespace Web.Pages
{
    [Authorize(Roles ="Privileged, Administrator")]
    public partial class Generate : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }
        
        private readonly UpdateTrigger _progressUpdated = new UpdateTrigger();

        private int           _blacklistRuleCount;
        private List<string>? _blacklistRuleSources;
        private List<string>  _progress = new List<string>();

        protected override void OnInitialized()
        {
            _blacklistRuleCount   = ConfigModel.BlacklistRuleCount();
            _blacklistRuleSources = ConfigModel.Read()?.Sources;

            Common.ContraCoreClient.OnGenRulesCallback += OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   += OnGenRulesChange;
        }

        private void OnGenRulesCallback(string s)
        {
            _progress.Add(s);
            _progressUpdated.Trigger();
        }

        private void OnGenRulesChange()
        {
            _progressUpdated.Trigger();
        }

        private async Task GenRules()
        {
            _progress = new List<string>();

            Common.ContraCoreClient.OnGenRulesChange += () =>
            {
                if (!Common.ContraCoreClient.GeneratingRules)
                {
                    _blacklistRuleCount = ConfigModel.BlacklistRuleCount();
                }

                _progressUpdated.Trigger();
            };

            await Common.ContraCoreClient.GenRules();
        }

        private bool AllowRuleGen()
        {
            UserRole.Roles role = Utility.GetRole(AuthenticationStateTask).Result;
            if (role != UserRole.Roles.Administrator) 
                return true;

            return !Common.ContraCoreClient.Connected || Common.ContraCoreClient.GeneratingRules;
        }

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.Generate.Dispose()");
            Common.ContraCoreClient.OnGenRulesCallback -= OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   -= OnGenRulesChange;
        }
    }
}