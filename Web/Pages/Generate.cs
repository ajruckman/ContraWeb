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
    [Authorize(Roles = "Privileged, Administrator")]
    public partial class Generate : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        private readonly UpdateTrigger _ruleGenProgressTrigger = new UpdateTrigger();
        private readonly UpdateTrigger _ouiGenProgressTrigger  = new UpdateTrigger();

        private int           _blacklistRuleCount;
        private List<string>? _blacklistRuleSources;
        private List<string>  _ruleGenProgress = new List<string>();

        private int          _ouiCount;
        private List<string> _ouiGenProgress = new List<string>();

        protected override void OnInitialized()
        {
            _blacklistRuleCount   = ConfigModel.BlacklistRuleCount();
            _blacklistRuleSources = ConfigModel.Read()?.Sources;

            Common.ContraCoreClient.OnGenRulesCallback += OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   += OnGenRulesChange;

            _ouiCount = OUIModel.OUICount();

            Common.ContraCoreClient.OnGenOUICallback += OnGenOUICallback;
            Common.ContraCoreClient.OnGenOUIChange   += OnGenOUIChange;
        }

        private void OnGenRulesCallback(string s)
        {
            _ruleGenProgress.Add(s);
            _ruleGenProgressTrigger.Trigger();
        }

        private void OnGenRulesChange()
        {
            if (!Common.ContraCoreClient.GeneratingRules)
            {
                _blacklistRuleCount = ConfigModel.BlacklistRuleCount();
            }
            _ruleGenProgressTrigger.Trigger();
        }

        private async Task GenRules()
        {
            if (!AllowGenRule())
                return;

            _ruleGenProgress = new List<string>();

            await Common.ContraCoreClient.GenRules();
        }

        private void OnGenOUICallback(string s)
        {
            _ouiGenProgress.Add(s);
            _ouiGenProgressTrigger.Trigger();
        }

        private void OnGenOUIChange()
        {
            if (!Common.ContraCoreClient.GeneratingRules)
            {
                _ouiCount = OUIModel.OUICount();
            }
            _ouiGenProgressTrigger.Trigger();
        }

        private async Task GenOUI()
        {
            if (!AllowGenOUI())
                return;

            _ouiGenProgress = new List<string>();

            await Common.ContraCoreClient.GenOUI();
        }

        private bool AllowGenRule()
        {
            UserRole.Roles role = Utility.GetRole(AuthenticationStateTask!).Result;
            if (role != UserRole.Roles.Administrator)
                return false;

            return Common.ContraCoreClient.Connected && !Common.ContraCoreClient.GeneratingRules;
        }

        private bool AllowGenOUI()
        {
            UserRole.Roles role = Utility.GetRole(AuthenticationStateTask!).Result;
            if (role != UserRole.Roles.Administrator)
                return false;

            return Common.ContraCoreClient.Connected && !Common.ContraCoreClient.GeneratingOUI;
        }

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.Generate.Dispose()");
            Common.ContraCoreClient.OnGenRulesCallback -= OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   -= OnGenRulesChange;
            Common.ContraCoreClient.OnGenOUICallback   -= OnGenOUICallback;
            Common.ContraCoreClient.OnGenOUIChange     -= OnGenOUIChange;
        }
    }
}