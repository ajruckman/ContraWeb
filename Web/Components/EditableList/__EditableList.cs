using Microsoft.AspNetCore.Components;

namespace Web.Components.EditableList
{
    public partial class __EditableList
    {
        [Parameter]
        public EditableList EditableList { get; set; }

        private string _newValue = "";

        private void Add()
        {
            if (_newValue.Length > 0)
            {
                EditableList.Add(_newValue);
                _newValue = "";
            }
        }
    }
}