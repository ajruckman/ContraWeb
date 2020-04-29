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
        public string? ClientIP { get; set; }

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
                () => _clientRole == UserRole.Roles.Privileged ? WhitelistModel.List(ClientIP!) : WhitelistModel.List(),
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
}