#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Components.EditableList
{
    public partial class __EditableList
    {
        private string                _newValue = "";
        private (Validation, MarkupString)? _validation;

        public string NewValue
        {
            get => _newValue;
            set
            {
                _newValue = value;
                if (EditableList.Validated)
                    _validation = EditableList.Validate(_newValue);
            }
        }

        [Parameter]
        public EditableList EditableList { get; set; }

        protected override void OnParametersSet()
        {
            if (EditableList.Validated)
                _validation = EditableList.Validate(_newValue);
            _newValue = EditableList.DefaultValue;
        }

        private void Add()
        {
            EditableList.Add(NewValue);
            NewValue = "";
        }

        private bool CanAdd()
        {
            if (NewValue.Length == 0) return false;

            if (EditableList.Validated)
            {
                (Validation, MarkupString)? validation = EditableList.Validate(NewValue);
                if (validation?.Item1 == Validation.Invalid)
                    return false;
            }

            return true;
        }

        private void OnKeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                if (CanAdd())
                    Add();
        }
    }
}