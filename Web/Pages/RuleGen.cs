using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Superset.Web.State;

namespace Web.Pages
{
    public partial class RuleGen
    {
        private bool         _processing;
        private List<string> _progress;
        private int          _blacklistRuleCount;

        private readonly UpdateTrigger _progressUpdated = new UpdateTrigger();

        protected override void OnInitialized()
        {
            _blacklistRuleCount = ConfigController.BlacklistRuleCount();
        }

        private async Task GenRules()
        {
            _processing = true;
            _progress   = new List<string>();

            Common.ContraCoreClient.OnGenRulesCallback += s =>
            {
                Console.WriteLine(s);
                _progress.Add(s);
                _progressUpdated.Trigger();
            };

            Common.ContraCoreClient.OnGenRulesChange += () =>
            {
                _blacklistRuleCount = ConfigController.BlacklistRuleCount();
                _progressUpdated.Trigger();
            };

            await Common.ContraCoreClient.GenRules();
        }
    }
}