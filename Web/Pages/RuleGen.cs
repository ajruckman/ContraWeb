#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Superset.Web.State;
using Web.Components.EditableList;

namespace Web.Pages
{
    public partial class RuleGen : IDisposable
    {
        private readonly UpdateTrigger _progressUpdated = new UpdateTrigger();

        private int           _blacklistRuleCount;
        private List<string>? _blacklistRuleSources;
        private List<string>  _progress = new List<string>();
        
        private Whitelist _newWhitelist;

        private string _expiresDate = DateTime.Now.Add(TimeSpan.FromDays(7)).ToString("yyyy-MM-dd");
        private string _expiresTime = DateTime.Now.ToString("HH:mm");

        private void OnPatternChange(ChangeEventArgs args)
        {
            Console.WriteLine("=> " + args.Value);
        }

        private void OnExpiresDateChange(ChangeEventArgs args)
        {
            Console.WriteLine("=> " + args.Value);
        }

        private void OnExpiresTimeChange(ChangeEventArgs args)
        {
            Console.WriteLine("=> " + args.Value);
        }


        private EditableList _newHostnameList = new EditableList();


        // private string       _newHostname = "";
        // private List<string> _hostnames   = new List<string>();
        //
        // private void AddHostname(MouseEventArgs args)
        // {
        //     if (string.IsNullOrEmpty(_newHostname)) return;
        //
        //     if (!_hostnames.Contains(_newHostname))
        //         _hostnames.Add(_newHostname);
        //
        //     _newHostname = "";
        // }
        //
        // private void RemoveHostname(string hostname)
        // {
        //     _hostnames.RemoveAll(v => v == hostname);
        // }


        protected override void OnInitialized()
        {
            _blacklistRuleCount   = ConfigModel.BlacklistRuleCount();
            _blacklistRuleSources = ConfigModel.Read()?.Sources;

            Common.ContraCoreClient.OnGenRulesCallback += OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   += OnGenRulesChange;

            _newWhitelist = new Whitelist();
        }

        private void OnGenRulesCallback(string s)
        {
            Console.WriteLine(s);
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

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.RuleGen.Dispose()");
            Common.ContraCoreClient.OnGenRulesCallback -= OnGenRulesCallback;
            Common.ContraCoreClient.OnGenRulesChange   -= OnGenRulesChange;
        }
    }
}