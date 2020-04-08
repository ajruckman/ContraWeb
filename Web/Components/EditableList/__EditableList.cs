using Infrastructure.Schema;
using Infrastructure.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Components.EditableList
{
    public partial class __EditableList
    {
        private string                      _newValue = "";
        private bool                        _hasInitialized;
        private (ValidationResult, MarkupString)? _validation;

        [Parameter]
        public EditableList EditableList { get; set; }

        protected override void OnParametersSet()
        {
            if (_hasInitialized) return;
            _hasInitialized = true;

            Validate();
            _newValue = EditableList.DefaultValue;
        }

        private void Add()
        {
            EditableList.Add(_newValue);
            _newValue = "";
        }

        private void Validate()
        {
            if (EditableList.Validated)
                _validation = EditableList.Validate(_newValue);
        }

        private void UpdateNewValue(ChangeEventArgs args)
        {
            _newValue = args.Value?.ToString() ?? "";
            Validate();
        }

        private bool CanAdd()
        {
            if (_newValue.Length == 0) return false;

            if (EditableList.Validated)
            {
                (ValidationResult, MarkupString)? validation = EditableList.Validate(_newValue);
                if (validation?.Item1 == ValidationResult.Invalid)
                    return false;
            }

            return true;
        }

        private void OnKeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                if (CanAdd())
                {
                    Add();
                    Validate();
                }
        }
    }
}