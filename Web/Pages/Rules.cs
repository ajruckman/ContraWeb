#nullable enable

using System;
using System.Net;
using System.Text.RegularExpressions;
using FlareSelect;
using Infrastructure.Model;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Web.Components.EditableList;

namespace Web.Pages
{
    public partial class Rules
    {
        // https://stackoverflow.com/a/106223/9911189
        private readonly Regex _matchIPv4Regex =
            new Regex(
                @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$",
                RegexOptions.Compiled);

        private readonly Regex _matchIPv6Regex =
            new Regex(
                @"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$",
                RegexOptions.Compiled);

        // https://stackoverflow.com/a/106223/9911189
        private readonly Regex _validHostnameRegex =
            new Regex(
                @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$",
                RegexOptions.Compiled);

        private string _pattern = "";
        
        private DateTime? _expiresDate; // = DateTime.Now.Add(TimeSpan.FromDays(7));
        private string?   _expiresInvalidReason;
        private TimeSpan? _expiresTime; // = DateTime.Now.Add(TimeSpan.FromMinutes(5)).TimeOfDay;
        private bool?     _isExpiresValid;

        private bool?         _isPatternValid;
        private EditableList? _newHostnameList;
        private EditableList? _newIPsList;
        private EditableList? _newSubnetsList;
        private Whitelist     _newWhitelist = new Whitelist();

        private FlareSelector? _vendorSelector;

        private void OnPatternChange(ChangeEventArgs args)
        {
            _pattern = args.Value?.ToString() ?? ""; 
            ValidatePattern();
            Console.WriteLine("=> " + args.Value);
        }

        private void ValidatePattern()
        {
            if (string.IsNullOrEmpty(_pattern))
                _isPatternValid = null;
            else
                _isPatternValid = Utility.ValidateRegex(_pattern);

            _newWhitelist.Pattern = _pattern;
        }

        private void OnExpiresDateChange(ChangeEventArgs args)
        {
            Console.WriteLine("=> " + args.Value);
            string? date = args.Value.ToString();
            if (string.IsNullOrEmpty(date))
            {
                _expiresDate = null;
            }
            else
            {
                _expiresDate = DateTime.Parse(date);
            }

            ValidateExpirationDateTime();
            StateHasChanged();
        }

        private string ExpiresDateValue()
        {
            return _expiresDate == null
                ? ""
                : _expiresDate.Value.ToString("yyyy-MM-dd");
        }

        private void OnExpiresTimeChange(ChangeEventArgs args)
        {
            Console.WriteLine("=> " + args.Value);
            string? time = args.Value.ToString();
            if (string.IsNullOrEmpty(time))
            {
                _expiresTime = null;
            }
            else
            {
                _expiresTime = DateTime.Parse(time).TimeOfDay;
            }

            ValidateExpirationDateTime();
            StateHasChanged();
        }

        private string ExpiresTimeValue()
        {
            return _expiresTime == null
                ? ""
                : DateTime.MinValue.Add(_expiresTime.Value).ToString("HH:mm");
        }

        private void SetExpirationAfterTimespan(TimeSpan timeSpan)
        {
            if (timeSpan != TimeSpan.Zero)
            {
                _expiresDate = DateTime.Now.Add(timeSpan).Date;
                _expiresTime = DateTime.Now.Add(timeSpan).TimeOfDay;
            }
            else
            {
                _expiresDate = null;
                _expiresTime = null;
            }

            ValidateExpirationDateTime();
        }

        private void ValidateExpirationDateTime()
        {
            if (_expiresDate == null)
            {
                if (_expiresTime != null)
                {
                    _isExpiresValid       = false;
                    _expiresInvalidReason = "Time is set but date is not";
                    return;
                }

                _isExpiresValid = null;
                return;
            }

            if (_expiresDate.Value.Date < DateTime.Now.Date)
            {
                _isExpiresValid       = false;
                _expiresInvalidReason = "Date is in the past";
                return;
            }

            if (_expiresTime == null)
            {
                if (_expiresDate != null)
                {
                    _isExpiresValid       = false;
                    _expiresInvalidReason = "Date is set but time is not";
                    return;
                }

                _isExpiresValid = null;
                return;
            }

            DateTime composite = _expiresDate.Value.Add(_expiresTime.Value);

            if (composite < DateTime.Now)
            {
                _isExpiresValid       = false;
                _expiresInvalidReason = "Time is in the past";
            }
            else
            {
                _isExpiresValid = true;
            }
        }

        protected override void OnInitialized()
        {
            _newIPsList = new EditableList(validator: s =>
                {
                    if (s.Length == 0)
                        return (Validation.Undefined, new MarkupString(""));

                    if (_matchIPv4Regex.IsMatch(s))
                        return (Validation.Valid, new MarkupString("Valid IP"));

                    if (_matchIPv6Regex.IsMatch(s))
                        return (Validation.Valid, new MarkupString("Valid IP"));

                    return (Validation.Invalid, new MarkupString("Invalid IP"));
                },
                placeholder: "Add an IP to whitelist");

            _newSubnetsList = new EditableList(
                validator: s =>
                {
                    if (s.Length == 0)
                        return (Validation.Undefined, new MarkupString(""));

                    bool valid = IPNetwork.TryParse(s, out IPNetwork parsed);

                    if (valid)
                        return (Validation.Valid, new MarkupString($"Parsed: {parsed}"));

                    return (Validation.Invalid, new MarkupString("Parse failure"));
                },
                transformer: s => IPNetwork.Parse(s).ToString(),
                placeholder: "Add a subnet to whitelist"
            );

            _newHostnameList = new EditableList(validator: s =>
                {
                    if (s.Length == 0)
                        return (Validation.Undefined, new MarkupString(""));

                    if (_validHostnameRegex.IsMatch(s))
                        return (Validation.Valid, new MarkupString("Valid hostname"));

                    return (Validation.Warning, new MarkupString("Invalid hostname"));
                },
                placeholder: "Add a hostname to whitelist");

            _vendorSelector = new FlareSelector(
                _ => OUIModel.Options(),
                multiple: true,
                minSearchTermLength: 2
            );

            ValidateExpirationDateTime();

            //

            Log fromLog = LogActionService.GetAndUnset();
            if (fromLog != null)
            {
                Console.WriteLine("From log: " + fromLog.Question);
                string pattern = @"(?:^|.+\.)" + fromLog.Question.Replace(".", @"\.") + "$";
                _pattern = pattern;
                ValidatePattern();
            }
        }
    }
}