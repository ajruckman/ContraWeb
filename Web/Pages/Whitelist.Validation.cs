using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;
using Superset.Web.State;
using Superset.Web.Validation;

namespace Web.Pages
{
    public partial class Whitelist
    {
        private readonly UpdateTrigger _onInputValidation = new UpdateTrigger();
        private          bool          _isExpiresValid;

        private Validator<ValidationResult>? _validator;
        private Validator<ValidationResult>? _overallValidator;

        private void InitValidators()
        {
            _validator = new Validator<ValidationResult>();

            _overallValidator = new Validator<ValidationResult>(() =>
            {
                if (_validator!.AnyOfType(ValidationResult.Invalid))
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Inputs invalid")};

                bool a = Rule().IPs?.Any()       == true;
                bool b = Rule().Subnets?.Any()   == true;
                bool c = Rule().Hostnames?.Any() == true;
                bool d = Rule().MACs?.Any()      == true;
                bool e = Rule().Vendors?.Any()   == true;

                return a || b || c || d || e
                    ? new[] {new Validation<ValidationResult>(ValidationResult.Valid, "All OK")}
                    : new[]
                    {
                        new Validation<ValidationResult>(ValidationResult.Invalid, "At least 1 whitelist condition is required")
                    };
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

            _validator.Register("Expires", ValidateExpiresDateTime);

            _validator.Validate();
            _overallValidator.Validate();
        }

        private IEnumerable<Validation<ValidationResult>> ValidateExpiresDateTime()
        {
            DateTime? ed = !_editing ? _newExpiresDate : _editExpiresDate;
            TimeSpan? et = !_editing ? _newExpiresTime : _editExpiresTime;

            Rule().Expires = null;

            if (ed == null)
            {
                Rule().Expires = null;
                if (et == null)
                {
                    _isExpiresValid = true;
                    return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "Rule will not expire")};
                }
                else
                {
                    _isExpiresValid = false;
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Time is set but date is not")};
                }
            }

            if (ed.Value.Date < DateTime.Now.Date)
            {
                _isExpiresValid = false;
                return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Date is in the past")};
            }

            if (et == null)
            {
                Rule().Expires = null;
                if (ed == null)
                {
                    _isExpiresValid = true;
                    return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "Rule will not expire")};
                }
                else
                {
                    _isExpiresValid = false;
                    return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Date is set but time is not")};
                }
            }

            DateTime composite = ed.Value.Add(et.Value);

            if (composite < DateTime.Now)
            {
                _isExpiresValid     = false;
                Rule().Expires = null;
                return new[] {new Validation<ValidationResult>(ValidationResult.Invalid, "Time is in the past")};
            }
            else
            {
                _isExpiresValid     = true;
                Rule().Expires = composite;
                return new[] {new Validation<ValidationResult>(ValidationResult.Valid, "Expiration time valid")};
            }
        }

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

        public (ValidationResult, MarkupString) ValidateIP(string s)
        {
            if (s.Length == 0)
                return (ValidationResult.Undefined, new MarkupString(""));

            if (_matchIPv4Regex.IsMatch(s))
                return (ValidationResult.Valid, new MarkupString("Valid IP"));

            if (_matchIPv6Regex.IsMatch(s))
                return (ValidationResult.Valid, new MarkupString("Valid IP"));

            return (ValidationResult.Invalid, new MarkupString("Invalid IP"));
        }

        public (ValidationResult, MarkupString) ValidateSubnet(string s)
        {
            if (s.Length == 0)
                return (ValidationResult.Undefined, new MarkupString(""));

            bool valid = IPNetwork.TryParse(s, out IPNetwork parsed);

            if (valid)
                return (ValidationResult.Valid, new MarkupString($"Parsed: {parsed}"));

            return (ValidationResult.Invalid, new MarkupString("Parse failure"));
        }

        public (ValidationResult, MarkupString) ValidateHostname(string s)
        {
            if (s.Length == 0)
                return (ValidationResult.Undefined, new MarkupString(""));

            if (_validHostnameRegex.IsMatch(s))
                return (ValidationResult.Valid, new MarkupString("Valid hostname"));

            return (ValidationResult.Warning, new MarkupString("Invalid hostname"));
        }

        public (ValidationResult, MarkupString) ValidateMAC(string s)
        {
            if (s.Length == 0)
                return (ValidationResult.Undefined, new MarkupString(""));

            return Utility.ValidateMAC(s)
                ? (ValidationResult.Valid, new MarkupString("Valid MAC"))
                : (ValidationResult.Invalid, new MarkupString("Invalid MAC"));
        }
    }
}