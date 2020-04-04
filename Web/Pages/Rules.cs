using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FS3;
using FT3;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;
using Superset.Utilities;
using Superset.Web.State;
using Web.Components.EditableList;

namespace Web.Pages
{
    public partial class Rules
    {
        private WhitelistController _whitelistController;

        private EditableList? _newIPsList;
        private EditableList? _newSubnetsList;
        private EditableList? _newHostnameList;

        private readonly UpdateTrigger _onPostLoad = new UpdateTrigger();
        private          bool          _doPostLoad;

        private FlareSelector<string>? _vendorSelector;

        private FlareTable<Whitelist> _whitelistTable;

        // private string     _newPattern             = "";
        private Debouncer<string>? _patternChangeDebouncer = null;

        private async Task OnPatternChange(ChangeEventArgs args)
        {
            // _newPattern = args.Value?.ToString() ?? "";
            _patternChangeDebouncer.Reset(args.Value?.ToString() ?? "");
        }

        private void OnExpiresDateChange(ChangeEventArgs args)
        {
            _whitelistController.UpdateExpiresDate(args.Value?.ToString() ?? "");
        }

        private void OnExpiresTimeChange(ChangeEventArgs args)
        {
            _whitelistController.UpdateExpiresTime(args.Value?.ToString());
        }

        protected override void OnInitialized()
        {
            _whitelistController = new WhitelistController(StateHasChanged);

            _patternChangeDebouncer = new Debouncer<string>(pattern =>
            {
                Console.WriteLine("New value: " + pattern);
                _whitelistController.UpdatePattern(pattern);
                InvokeAsync(StateHasChanged);
            }, "", 200);

            _newIPsList = new EditableList(validator: _whitelistController.ValidateIP,
                placeholder: "Add an IP to whitelist");
            _newIPsList.OnUpdate += ips => _whitelistController.UpdateIPList(ips);

            _newSubnetsList = new EditableList(
                validator: _whitelistController.ValidateSubnet,
                transformer: s => IPNetwork.Parse(s).ToString(),
                placeholder: "Add a subnet to whitelist"
            );
            _newSubnetsList.OnUpdate += subnets => _whitelistController.UpdateSubnetList(subnets);

            _newHostnameList = new EditableList(validator: _whitelistController.ValidateHostname,
                placeholder: "Add a hostname to whitelist");
            _newHostnameList.OnUpdate += hostnames => _whitelistController.UpdateHostnameList(hostnames);

            Log fromLog = LogActionService.GetAndUnset();
            if (fromLog != null)
            {
                Console.WriteLine("From log: "                                        + fromLog.Question);
                string pattern = @"(?:^|.+\.)" + fromLog.Question.Replace(".", @"\.") + "$";
                _whitelistController.UpdatePattern(pattern);
            }

            //

            _whitelistTable = new FlareTable<Whitelist>(WhitelistModel.List, sessionStorage: SessionStorageService,
                identifier: "Rules.WhitelistTable");

            _whitelistTable.RegisterColumn(nameof(Whitelist.ID));
            _whitelistTable.RegisterColumn(nameof(Whitelist.Pattern));
            _whitelistTable.RegisterColumn(nameof(Whitelist.Expires));

            _whitelistTable.OnRowClick += whitelist => { };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            Task.Run(() =>
            {
                Thread.Sleep(1000);
                _vendorSelector = new FlareSelector<string>(
                    OUIModel.Options,
                    multiple: true,
                    minFilterValueLength: 3,
                    emptyPlaceholder: "Add an OUI to whitelist (ex: Apple, Cisco Systems Inc.)"
                );
                _vendorSelector.OnSelect += selected =>
                {
                    _whitelistController.UpdateVendorList(selected.Select(v => v.ID).ToList());
                };
                _doPostLoad = true;
                _onPostLoad.Trigger();
            });

            await _whitelistTable.LoadSessionValues();
        }

        private void Commit()
        {
            _whitelistController.Commit();
        }
    }
}