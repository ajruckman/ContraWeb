#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Model;
using Superset.Web.State;

namespace Web.Pages
{
    public partial class RuleGen : IDisposable
    {
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

        private bool CanGenRules => Common.ContraCoreClient.Connected && !Common.ContraCoreClient.GeneratingRules;

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.RuleGen.Dispose()");
            Common.ContraCoreClient.OnGenRulesCallback -= OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   -= OnGenRulesChange;
        }
    }
}