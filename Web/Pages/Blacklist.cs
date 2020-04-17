using System;
using System.Threading.Tasks;
using FlareTables;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Superset.Utilities;
using Superset.Web.State;
using Superset.Web.Validation;

namespace Web.Pages
{
    [Authorize(Roles = "Privileged, Administrator")]
    public partial class Blacklist
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        private UserRole.Roles _clientRole;
        private bool            _processing;

        private Infrastructure.Schema.Blacklist              _newRule;
        private FlareTable<Infrastructure.Schema.Blacklist>? _blacklistTable;
        private bool                                         _loaded = false;

        private readonly UpdateTrigger                _onInputValidation = new UpdateTrigger();
        private          Validator<ValidationResult>? _validator;
        private          Debouncer<string>?           _patternChangeDebouncer;

        private Infrastructure.Schema.Blacklist Rule()
        {
            return _newRule;
        }

        protected override void OnInitialized()
        {
            // Console.WriteLine("Initialized <><><> " + ClientRoleGetter.Invoke());

            _clientRole = Utility.GetRole(AuthenticationStateTask ?? throw new ArgumentNullException()).Result;
            
            _newRule = new Infrastructure.Schema.Blacklist();

            Log? fromLog = LogActionService.GetAndUnset();
            if (fromLog != null)
            {
                Console.WriteLine("From log: "                                        + fromLog.Question);
                string pattern = @"(?:^|.+\.)" + fromLog.Question.Replace(".", @"\.") + "$";
                _newRule.Pattern = pattern;
            }

            _validator = new Validator<ValidationResult>();
            //     () =>
            // {
            //     if (_clientRole != UserRole.Roles.Administrator)
            //         return new[]
            //         {
            //             new Validation<ValidationResult>(ValidationResult.Warning,
            //                 "You are not permitted to create blacklist rules")
            //         };
            //     
            //     if (string.IsNullOrEmpty(Rule().Pattern))
            //         return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Inputs invalid")};
            //
            //     return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "All OK")};
            // });

            _validator.Register("Pattern", () =>
            {
                if (_clientRole != UserRole.Roles.Administrator)
                    return new[]
                    {
                        new Validation<ValidationResult>(ValidationResult.Warning,
                            "You are not permitted to create blacklist rules")
                    };

                if (string.IsNullOrEmpty(Rule().Pattern))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Required")};

                if (!Utility.ValidateRegex(Rule().Pattern))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Invalid regular expression")};

                return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "Valid")};
            });

            _patternChangeDebouncer = new Debouncer<string>(pattern =>
            {
                _validator!.Validate();
                _onInputValidation.Trigger();
                InvokeAsync(StateHasChanged);
            }, "", 200);

            _validator.Validate();

            Task.Run(() =>
            {
                try
                {
                    BuildBlacklistTable();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private void BuildBlacklistTable()
        {
            _blacklistTable = new FlareTable<Infrastructure.Schema.Blacklist>(
                BlacklistModel.List,
                monospace: true
            );

            _blacklistTable.RegisterColumn(nameof(Infrastructure.Schema.Blacklist.ID), width: "80px");
            _blacklistTable.RegisterColumn(nameof(Infrastructure.Schema.Blacklist.Pattern));
            _blacklistTable.RegisterColumn(nameof(Infrastructure.Schema.Blacklist.Expires));
            _blacklistTable.RegisterColumn("_Remove", sortable: false, filterable: false, displayName: "", width: "82px");

            _loaded = true;
            InvokeAsync(StateHasChanged);
        }

        private async Task OnPatternChange(ChangeEventArgs args)
        {
            var pattern = args.Value?.ToString() ?? "";
            Rule().Pattern = pattern;
            _patternChangeDebouncer!.Reset(pattern);
        }

        private async Task Commit()
        {
            _processing = true;

            if (_validator!.AnyOfType(ValidationResult.Invalid))
            {
                _processing = false;
                return;
            }

            BlacklistModel.Submit(_newRule);

            _newRule = new Infrastructure.Schema.Blacklist();
            _validator!.Validate();

            await ReloadBlacklist();

            _blacklistTable?.InvalidateData();
        }


        private async Task Remove(Infrastructure.Schema.Blacklist row)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Remove rule '{row.Pattern}'?"});
            if (confirmed)
            {
                BlacklistModel.Remove(row);
                _blacklistTable!.InvalidateData();
                await ReloadBlacklist();
            }
        }

        private async Task ReloadBlacklist()
        {
            _processing = false;

            bool success = await Common.ContraCoreClient.ReloadBlacklist();
            if (!success)
                Console.WriteLine("Not reloading blacklist after adding/removing rule because ContraCore is disconnected.");
        }
    }
}