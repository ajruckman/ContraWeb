using System.Threading.Tasks;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Superset.Utilities;
using Superset.Web.State;
using Superset.Web.Validation;

namespace Web.Pages
{
    public partial class Blacklist
    {
        private          Validator<ValidationResult>? _validator;
        private readonly UpdateTrigger                _onInputValidation = new UpdateTrigger();

        private Debouncer<string>? _patternChangeDebouncer;

        private Infrastructure.Schema.Blacklist _newRule;
        private bool                            _processing;

        private Infrastructure.Schema.Blacklist Rule()
        {
            return _newRule;
        }

        protected override void OnInitialized()
        {
            _newRule = new Infrastructure.Schema.Blacklist();
            
            _validator = new Validator<ValidationResult>(() =>
            {
                if (string.IsNullOrEmpty(Rule().Pattern))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Inputs invalid")};

                return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "All OK")};
            });
            
            _validator.Register("Pattern", () =>
            {
                return new[]
                {
                    string.IsNullOrEmpty(Rule().Pattern)
                        ? new Validation<ValidationResult>(ValidationResult.Invalid, "Required")
                        : Utility.ValidateRegex(Rule().Pattern)
                            ? new Validation<ValidationResult>(ValidationResult.Valid,   "Valid")
                            : new Validation<ValidationResult>(ValidationResult.Invalid, "Invalid regular expression")
                };
            });

            _patternChangeDebouncer = new Debouncer<string>(pattern =>
            {
                _validator!.Validate();
                _onInputValidation.Trigger();
                InvokeAsync(StateHasChanged);
            }, "", 200);
            
            _validator.Validate();
        }

        private async Task OnPatternChange(ChangeEventArgs args)
        {
            var pattern = args.Value?.ToString() ?? "";
            Rule().Pattern = pattern;
            _patternChangeDebouncer!.Reset(pattern);
        }

        private async Task Commit() { }
    }
}