using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using FlareSelect;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Superset.Utilities;
using Superset.Web.State;
using Superset.Web.Validation;
using Web.Components.EditableList;

namespace Web.Pages
{
    public partial class Whitelist
    {
        private Debouncer<string>? _patternChangeDebouncer;

        private EditableList? _newIPsList;
        private EditableList? _newSubnetsList;
        private EditableList? _newHostnameList;
        private EditableList? _newMACsList;

        private EditableList? _editIPsList;
        private EditableList? _editSubnetsList;
        private EditableList? _editHostnameList;
        private EditableList? _editMACsList;

        private FlareSelector<string>? _vendorSelector;

        private readonly UpdateTrigger _onInputReset = new UpdateTrigger();

        public void InitInputs()
        {
            _patternChangeDebouncer = new Debouncer<string>(pattern =>
            {
                UpdateOverall();
                InvokeAsync(StateHasChanged);
            }, "", 200);
            
            // New inputs

            _newIPsList = new EditableList(
                validator: ValidateIP,
                placeholder: "Add an IP to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _newIPsList.OnUpdate += UpdateIPList;

            if (!AllowCreateRuleForOthers())
            {
                _newIPsList.Add(ClientIP!);
            }

            _newSubnetsList = new EditableList(
                validator: ValidateSubnet,
                transformer: s => IPNetwork.Parse(s).ToString(),
                placeholder: "Add a subnet to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _newSubnetsList.OnUpdate += UpdateSubnetList;

            _newHostnameList = new EditableList(
                validator: ValidateHostname,
                placeholder: "Add a hostname to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _newHostnameList.OnUpdate += UpdateHostnameList;

            _newMACsList = new EditableList(
                validator: ValidateMAC,
                placeholder: "Add a MAC to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers(),
                transformer: Utility.FormatMAC
            );
            _newMACsList.OnUpdate += UpdateMACList;
            
            // Edit inputs
            
            _editIPsList = new EditableList(
                validator: ValidateIP,
                placeholder: "Add an IP to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _editIPsList.OnUpdate += UpdateIPList;

            if (!AllowCreateRuleForOthers())
            {
                _editIPsList.Add(ClientIP!);
            }

            _editSubnetsList = new EditableList(
                validator: ValidateSubnet,
                transformer: s => IPNetwork.Parse(s).ToString(),
                placeholder: "Add a subnet to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _editSubnetsList.OnUpdate += UpdateSubnetList;

            _editHostnameList = new EditableList(
                validator: ValidateHostname,
                placeholder: "Add a hostname to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers()
            );
            _editHostnameList.OnUpdate += UpdateHostnameList;

            _editMACsList = new EditableList(
                validator: ValidateMAC,
                placeholder: "Add a MAC to whitelist",
                isDisabled: () => _processing || !AllowCreateRuleForOthers(),
                transformer: Utility.FormatMAC
            );
            _editMACsList.OnUpdate += UpdateMACList;
            
        }

        private readonly UpdateTrigger _onPostLoad = new UpdateTrigger();
        private          bool          _doPostLoad;

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
                    emptyPlaceholder: "Add an OUI to whitelist (ex: Apple, Cisco Systems Inc.)",
                    isDisabled: () => _processing || !AllowCreateRuleForOthers()
                );
                _vendorSelector.OnSelect += selected => { UpdateVendorList(selected.Select(v => v.ID).ToList()); };
                _doPostLoad              =  true;
                _onPostLoad.Trigger();
                _onRowChange.Trigger();
            });

            await _whitelistTable!.LoadSessionValues();
        }

        private void UpdateOverall()
        {
            _validator!.Validate();
            _overallValidator!.Validate();
            _onInputValidation.Trigger();
        }

        private async Task OnPatternChange(ChangeEventArgs args)
        {
            var pattern = args.Value?.ToString() ?? "";
            Rule().Pattern = pattern;
            UpdateOverall();
            _patternChangeDebouncer!.Reset(pattern);
        }

        private string ExpiresDateString => Rule().Expires.HasValue
            ? Rule().Expires!.Value.ToString("yyyy-MM-dd")
            : "";

        private string ExpiresTimeString => Rule().Expires.HasValue
            ? DateTime.MinValue.Add(Rule().Expires!.Value.TimeOfDay).ToString("HH:mm")
            : "";

        private async Task OnExpiresDateChange(ChangeEventArgs args)
        {
            string v = args.Value?.ToString() ?? "";
            if (v == "")
            {
                if (!_editing)
                    _newExpiresDate = null;
                else
                    _editExpiresDate = null;
            }
            else if (!_editing)
            {
                if (!_editing)
                    _newExpiresDate = DateTime.Parse(v);
                else
                    _editExpiresDate = DateTime.Parse(v);
            }

            UpdateOverall();
        }

        private async Task OnExpiresTimeChange(ChangeEventArgs args)
        {
            string v = args.Value?.ToString() ?? "";
            if (v == "")
            {
                if (!_editing)
                    _newExpiresTime = null;
                else
                    _editExpiresTime = null;
            }
            else if (!_editing)
            {
                if (!_editing)
                    _newExpiresTime = DateTime.Parse(v).TimeOfDay;
                else
                    _editExpiresTime = DateTime.Parse(v).TimeOfDay;
            }

            UpdateOverall();
        }

        private async Task SetExpirationAfterTimespan(TimeSpan timeSpan)
        {
            if (timeSpan != TimeSpan.Zero)
            {
                if (!_editing)
                {
                    _newExpiresDate = DateTime.Now.Add(timeSpan).Date;
                    _newExpiresTime = DateTime.Now.Add(timeSpan).TimeOfDay;
                }
                else
                {
                    _editExpiresDate = DateTime.Now.Add(timeSpan).Date;
                    _editExpiresTime = DateTime.Now.Add(timeSpan).TimeOfDay;
                }
            }
            else
            {
                if (!_editing)
                {
                    _newExpiresDate = null;
                    _newExpiresTime = null;
                }
                else
                {
                    _editExpiresDate = null;
                    _editExpiresTime = null;
                }
            }

            _onInputReset.Trigger();
            UpdateOverall();
        }

        private void UpdateIPList(List<string> ips)
        {
            if (ips.Count == 0)
            {
                Rule().IPs = null;
                UpdateOverall();
                return;
            }

            Rule().IPs = new List<IPAddress>();
            foreach (string ip in ips)
                if (ip != "" && ValidateIP(ip).Item1 == ValidationResult.Valid)
                    Rule().IPs!.Add(IPAddress.Parse(ip));
            UpdateOverall();
        }

        private void UpdateSubnetList(List<string> subnets)
        {
            if (subnets.Count == 0)
            {
                Rule().Subnets = null;
                UpdateOverall();
                return;
            }

            Rule().Subnets = new List<IPNetwork>();
            foreach (string subnet in subnets)
                if (subnet != "")
                    Rule().Subnets!.Add(IPNetwork.Parse(subnet));

            UpdateOverall();
        }

        private void UpdateHostnameList(List<string> hostnames)
        {
            if (hostnames.Count == 0)
            {
                Rule().Hostnames = null;
                UpdateOverall();
                return;
            }

            Rule().Hostnames = hostnames;
            UpdateOverall();
        }

        private void UpdateMACList(List<string> macs)
        {
            if (macs.Count == 0)
            {
                Rule().MACs = null;
                UpdateOverall();
                return;
            }

            Rule().MACs = macs.Select(v => PhysicalAddress.Parse(v.Replace(":", ""))).ToList();
            UpdateOverall();
        }

        public void UpdateVendorList(List<string> vendors)
        {
            if (vendors.Count == 0)
            {
                Rule().Vendors = null;
                UpdateOverall();
                return;
            }

            Rule().Vendors = new List<string>();
            foreach (string vendor in vendors)
                if (vendor != "")
                    Rule().Vendors!.Add(vendor);

            UpdateOverall();
        }

        private bool AllowCreateRuleForOthers()
        {
            UserRole.Roles role = Utility.GetRole(AuthenticationStateTask!).Result;
            return role == UserRole.Roles.Administrator;
        }
    }
}