﻿using Web.Components.EditableList;

namespace Web.Pages
{
    public partial class EditableListDemo
    {
        EditableList _hostnameList = new EditableList(true, true);

        protected override void OnInitialized()
        {
            _hostnameList.Add("1010101010101");
            _hostnameList.Add("333");
            _hostnameList.Add("10101010101010101");
            _hostnameList.Add("88888888");
            _hostnameList.Add("55555");
            _hostnameList.Add("10101010101");
            _hostnameList.Add("101010101010101010");
            _hostnameList.Add("666666");
            _hostnameList.Add("1010101010101010");
            _hostnameList.Add("10101010101010101010");
            _hostnameList.Add("7777777");
            _hostnameList.Add("999999999");
            _hostnameList.Add("1010101010");
            _hostnameList.Add("101010101010101");
            _hostnameList.Add("22");
            _hostnameList.Add("1010101010101010101");
            _hostnameList.Add("101010101010101010101010101010101010101010101010101010101010101010101010");
            _hostnameList.Add("101010101010");
            _hostnameList.Add("1");
            _hostnameList.Add("10101010101010");
            _hostnameList.Add("4444");
            _hostnameList.Add("10101010101010101010101010101010101010101010101010101010");
            _hostnameList.Add("10101010101010101 0101010101010101010101010 10101010101010");
            _hostnameList.Add("10101010101010101 0101010101010101010101010 10101010101010 1010101010101010101010101010");
        }
    }
}