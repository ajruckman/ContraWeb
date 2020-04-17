using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlareTables;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Superset.Web.State;
using Superset.Web.Validation;

#pragma warning disable 1998
#pragma warning disable 4014

namespace Web.Pages
{
    [Authorize(Roles = "Privileged, Administrator")]
    public partial class Whitelist : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        [CascadingParameter]
        public string ClientIP { get; set; }

        private UserRole.Roles _clientRole;
        private bool           _editing;
        private bool           _processing;

        private Infrastructure.Schema.Whitelist? _newRule;
        private Infrastructure.Schema.Whitelist? _editRule;

        private DateTime? _newExpiresDate;
        private TimeSpan? _newExpiresTime;
        private DateTime? _editExpiresDate;
        private TimeSpan? _editExpiresTime;

        private          FlareTable<Infrastructure.Schema.Whitelist>? _whitelistTable;
        private readonly UpdateTrigger                                _updateWhitelistTable = new UpdateTrigger();
        private readonly UpdateTrigger                                _onRowChange          = new UpdateTrigger();

        private Infrastructure.Schema.Whitelist Rule()
        {
            return !_editing ? _newRule! : _editRule!;
        }

        protected override void OnInitialized()
        {
            _clientRole = Utility.GetRole(AuthenticationStateTask ?? throw new ArgumentNullException()).Result;

            _newRule = new Infrastructure.Schema.Whitelist();

            if (_clientRole != UserRole.Roles.Privileged && _clientRole != UserRole.Roles.Administrator)
                throw new Exception();

            Log? fromLog = LogActionService.GetAndUnset();
            if (fromLog != null)
            {
                Console.WriteLine("From log: "                                        + fromLog.Question);
                string pattern = @"(?:^|.+\.)" + fromLog.Question.Replace(".", @"\.") + "$";
                Rule().Pattern = pattern;
            }

            InitValidators();

            InitInputs();

            //

            _whitelistTable = new FlareTable<Infrastructure.Schema.Whitelist>(
                () => _clientRole == UserRole.Roles.Privileged ? WhitelistModel.List(ClientIP) : WhitelistModel.List(),
                sessionStorage: SessionStorageService,
                identifier: "Rules.WhitelistTable", monospace: true);

            _whitelistTable.RegisterColumn(nameof(Infrastructure.Schema.Whitelist.ID));
            _whitelistTable.RegisterColumn(nameof(Infrastructure.Schema.Whitelist.Pattern));
            _whitelistTable.RegisterColumn(nameof(Infrastructure.Schema.Whitelist.Expires));
            _whitelistTable.RegisterColumn("_Edit",   sortable: false, filterable: false, displayName: "", width: "56px");
            _whitelistTable.RegisterColumn("_Remove", sortable: false, filterable: false, displayName: "", width: "82px");

            _whitelistTable.OnRowClick += whitelist => { };

            BuildHeaders();
        }

        private void Commit()
        {
            _processing = true;

            if (_validator!.AnyOfType(ValidationResult.Invalid))
            {
                _processing = false;
                return;
            }

            if (!_editing)
            {
                WhitelistModel.Submit(_newRule!);

                _newRule = new Infrastructure.Schema.Whitelist();

                _newIPsList!.Empty();
                _newSubnetsList!.Empty();
                _newHostnameList!.Empty();
                _newMACsList!.Empty();
                _vendorSelector!.Deselect();

                SetExpirationAfterTimespan(TimeSpan.Zero);
            }
            else
            {
                WhitelistModel.Update(_editRule!);

                CancelEdit();
            }

            //     WhitelistModel.Submit(_newWhitelist);
            // OnCommit?.Invoke();
            //
            // _newWhitelist = new Whitelist();
            // return true;
            //
            // if ()
            // {
            //     _processing = false;
            //     _reloadMessages.Add("Failed to commit rule to database");
            //     _onReloadMessage.Trigger();
            // }

            ReloadWhitelist();

            _whitelistTable!.InvalidateData();
        }

        private async Task Edit(Infrastructure.Schema.Whitelist row)
        {
            _editRule = row;
            _editing  = true;

            _editIPsList!.Set(row.IPs?.Select(v => v.ToString())            ?? new string[] { });
            _editSubnetsList!.Set(row.Subnets?.Select(v => v.ToString())    ?? new string[] { });
            _editHostnameList!.Set(row.Hostnames?.Select(v => v.ToString()) ?? new string[] { });
            _editMACsList!.Set(row.MACs?.Select(Utility.FormatMAC)          ?? new string[] { });

            _vendorSelector!.Select(row.Vendors ?? new List<string>(), true);
            _vendorSelector!.UpdateFilterValue("");

            BuildHeaders();
            StateHasChanged();
            _onInputReset.Trigger();
            _onRowChange.Trigger();
        }

        private void CancelEdit()
        {
            _vendorSelector!.Select(_newRule!.Vendors ?? new List<string>(), true);
            _vendorSelector!.UpdateFilterValue("");

            _editing = false;
            _validator!.Validate();
            _overallValidator!.Validate();

            BuildHeaders();
            _onRowChange.Trigger();
        }

        private async Task Remove(Infrastructure.Schema.Whitelist row)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Remove rule '{row.Pattern}'?"});
            if (confirmed)
            {
                WhitelistModel.Remove(row);
                _whitelistTable!.InvalidateData();
                await ReloadWhitelist();
            }
        }

        private async Task ReloadWhitelist()
        {
            _processing = false;

            bool success = await Common.ContraCoreClient.ReloadWhitelist();
            if (!success)
                Console.WriteLine("Not reloading whitelist after adding/removing rule because ContraCore is disconnected.");
        }


        public void Dispose() { }
    }


    //     private          WhitelistController? _whitelistController;
    //     private readonly UpdateTrigger        _onWhitelistControllerValidation = new UpdateTrigger();
    //     private          bool                 _processing;
    //     private          bool                 _justReloaded;
    //     private readonly List<string>         _reloadMessages  = new List<string>();
    //     private readonly UpdateTrigger        _onReloadMessage = new UpdateTrigger();
    //
    //     private EditableList? _newIPsList;
    //     private EditableList? _newSubnetsList;
    //     private EditableList? _newHostnameList;
    //
    //     private readonly UpdateTrigger _onPostLoad = new UpdateTrigger();
    //     private          bool          _doPostLoad;
    //
    //     private FlareSelector<string>? _vendorSelector;
    //
    //     private FlareTable<Whitelist>? _whitelistTable;
    //
    //     private Whitelist? _editing;
    //
    //     private Debouncer<string>? _patternChangeDebouncer;
    //     
    //     
    //
    //     protected override void OnInitialized()
    //     {
    //         Common.ContraCoreClient.OnReloadWhitelistStatusCallback += AddMessage;
    //         Common.ContraCoreClient.OnReloadWhitelistErrorCallback  += AddMessage;
    //         
    //         _validator = new Validator<ValidationResult>();
    //         

    //         
    //         
    //
    //         _whitelistController = new WhitelistController(StateHasChanged);
    //         _whitelistController.OnValidation += () =>
    //         {
    //             if (_justReloaded)
    //             {
    //                 _justReloaded = false;
    //                 _onReloadMessage.Trigger();
    //             }
    //
    //             _onWhitelistControllerValidation.Trigger();
    //         };
    //
    //         _whitelistController.OnCommit += async () => await ReloadWhitelist();
    //
    //         _patternChangeDebouncer = new Debouncer<string>(pattern =>
    //         {
    //             Console.WriteLine("New value: " + pattern);
    //             _whitelistController.UpdatePattern(pattern);
    //             InvokeAsync(StateHasChanged);
    //         }, "", 200);
    //
    //         _newIPsList = new EditableList(
    //             validator: _whitelistController.ValidateIP,
    //             placeholder: "Add an IP to whitelist",
    //             isDisabled: () => _processing
    //         );
    //         _newIPsList.OnUpdate += ips => _whitelistController.UpdateIPList(ips);
    //
    //         _newSubnetsList = new EditableList(
    //             validator: _whitelistController.ValidateSubnet,
    //             transformer: s => IPNetwork.Parse(s).ToString(),
    //             placeholder: "Add a subnet to whitelist",
    //             isDisabled: () => _processing
    //         );
    //         _newSubnetsList.OnUpdate += subnets => _whitelistController.UpdateSubnetList(subnets);
    //
    //         _newHostnameList = new EditableList(
    //             validator: _whitelistController.ValidateHostname,
    //             placeholder: "Add a hostname to whitelist",
    //             isDisabled: () => _processing
    //         );
    //         _newHostnameList.OnUpdate += hostnames => _whitelistController.UpdateHostnameList(hostnames);
    //
    //         Log fromLog = LogActionService.GetAndUnset();
    //         if (fromLog != null)
    //         {
    //             Console.WriteLine("From log: "                                        + fromLog.Question);
    //             string pattern = @"(?:^|.+\.)" + fromLog.Question.Replace(".", @"\.") + "$";
    //             _whitelistController.UpdatePattern(pattern);
    //         }
    //
    //         //
    //

    //     }
    //
    //     private async Task ReloadWhitelist()
    //     {
    //         Thread.Sleep(100);
    //         _reloadMessages.Clear();
    //         _processing   = false;
    //         _justReloaded = true;
    //         _newIPsList!.Empty();
    //         _newSubnetsList!.Empty();
    //         _newHostnameList!.Empty();
    //         await Common.ContraCoreClient.ReloadWhitelist();
    //     }
    //
    //     private async Task OnPatternChange(ChangeEventArgs args)
    //     {
    //         _patternChangeDebouncer!.Reset(args.Value?.ToString() ?? "");
    //
    //         if (_justReloaded)
    //         {
    //             _justReloaded = false;
    //             _onReloadMessage.Trigger();
    //         }
    //     }
    //
    //     private void OnExpiresDateChange(ChangeEventArgs args)
    //     {
    //         _whitelistController!.UpdateExpiresDate(args.Value?.ToString() ?? "");
    //     }
    //
    //     private void OnExpiresTimeChange(ChangeEventArgs args)
    //     {
    //         _whitelistController!.UpdateExpiresTime(args.Value?.ToString());
    //     }
    //
    //     private void AddMessage(string message)
    //     {
    //         _reloadMessages.Add(message);
    //         _onReloadMessage.Trigger();
    //     }
    //
    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (!firstRender) return;
    //
    //     Task.Run(() =>
    //     {
    //         Thread.Sleep(1000);
    //         _vendorSelector = new FlareSelector<string>(
    //             OUIModel.Options,
    //             multiple: true,
    //             minFilterValueLength: 3,
    //             emptyPlaceholder: "Add an OUI to whitelist (ex: Apple, Cisco Systems Inc.)",
    //             isDisabled: () => _processing
    //         );
    //         _vendorSelector.OnSelect += selected =>
    //         {
    //             _whitelistController!.UpdateVendorList(selected.Select(v => v.ID).ToList());
    //         };
    //         _doPostLoad = true;
    //         _onPostLoad.Trigger();
    //     });
    //
    //     await _whitelistTable!.LoadSessionValues();
    // }
    //
    //     private void Commit()
    //     {
    //         _processing = true;
    //         if (!_whitelistController!.Commit())
    //         {
    //             _processing = false;
    //             _reloadMessages.Add("Failed to commit rule to database");
    //             _onReloadMessage.Trigger();
    //         }
    //         _whitelistTable!.InvalidateData();
    //     }
    //
    //     public void Dispose()
    //     {
    //         Common.ContraCoreClient.OnReloadWhitelistStatusCallback -= AddMessage;
    //         Common.ContraCoreClient.OnReloadWhitelistErrorCallback  -= AddMessage;
    //     }
    //
    //     // private async Task Edit(Whitelist row)
    //     // {
    //     //     _editing = row;
    //     //     _whitelistController!.Edit(row);
    //     //
    //     //     if (row.IPs != null)
    //     //         _newIPsList!.Set(row.IPs.Select(v => v.ToString()).ToList());
    //     //     else
    //     //         _newIPsList!.Empty();
    //     //
    //     //     if (row.Subnets != null)
    //     //         _newSubnetsList!.Set(row.Subnets.Select(v => v.ToString()).ToList());
    //     //     else
    //     //         _newSubnetsList!.Empty();
    //     //
    //     //     if (row.Hostnames != null)
    //     //         _newHostnameList!.Set(row.Hostnames);
    //     //     else
    //     //         _newHostnameList!.Empty();
    //     // }
    //
    //     private async Task Remove(Whitelist row)
    //     {
    //         var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Remove rule '{row.Pattern}'?"});
    //         if (confirmed)
    //         {
    //             WhitelistController.Remove(row);
    //             _whitelistTable!.InvalidateData();
    //             await ReloadWhitelist();
    //         }
    //     }
    // }
}