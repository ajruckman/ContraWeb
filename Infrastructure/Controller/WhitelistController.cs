using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;

namespace Infrastructure.Controller
{
    public class WhitelistController
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

        private Whitelist _newWhitelist = new Whitelist();

        // https://stackoverflow.com/a/106223/9911189
        private readonly Regex _validHostnameRegex =
            new Regex(
                @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$",
                RegexOptions.Compiled);

        private DateTime? _expiresDate; // = DateTime.Now.Add(TimeSpan.FromDays(7));

        // private DateTime? _expiresDateAndTime;
        private TimeSpan? _expiresTime; // = DateTime.Now.Add(TimeSpan.FromMinutes(5)).TimeOfDay;

        // private List<string> _ips       = new List<string>();
        // private List<string> _subnets   = new List<string>();
        // private List<string> _hostnames = new List<string>();
        // private List<string> _macs      = new List<string>();
        // private List<string> _vendors   = new List<string>();

        public WhitelistController(Action onValidation)
        {
            ValidateExpiresDateTime();
            OnValidation = onValidation;
        }

        public bool   IsPatternValid       { get; private set; }
        public bool?  IsExpiresValid       { get; private set; }
        public string ExpiresInvalidReason { get; private set; } = "";

        public string Pattern => _newWhitelist.Pattern;

        public string ExpiresDate => _expiresDate.HasValue ? _expiresDate.Value.ToString("yyyy-MM-dd") : "";
        public string ExpiresTime => _expiresTime.HasValue ? DateTime.MinValue.Add(_expiresTime.Value).ToString("HH:mm") : "";

        public event Action OnValidation;

        public void UpdatePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                IsPatternValid = false;
            else
                IsPatternValid = Utility.Utility.ValidateRegex(pattern);

            _newWhitelist.Pattern = pattern;
        }

        public void UpdateExpiresDate(string? date)
        {
            if (string.IsNullOrEmpty(date))
                _expiresDate = null;
            else
                _expiresDate = DateTime.Parse(date);

            ValidateExpiresDateTime();
            OnValidation.Invoke();
        }

        public void UpdateExpiresTime(string? time)
        {
            if (string.IsNullOrEmpty(time))
                _expiresTime = null;
            else
                _expiresTime = DateTime.Parse(time).TimeOfDay;

            ValidateExpiresDateTime();
            OnValidation.Invoke();
        }

        public void SetExpirationAfterTimespan(TimeSpan timeSpan)
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

            ValidateExpiresDateTime();
            OnValidation.Invoke();
        }

        private void ValidateExpiresDateTime()
        {
            if (_expiresDate == null)
            {
                if (_expiresTime != null)
                {
                    IsExpiresValid        = false;
                    _newWhitelist.Expires = null;
                    ExpiresInvalidReason  = "Time is set but date is not";
                    return;
                }

                IsExpiresValid        = null;
                _newWhitelist.Expires = null;
                return;
            }

            if (_expiresDate.Value.Date < DateTime.Now.Date)
            {
                IsExpiresValid        = false;
                _newWhitelist.Expires = null;
                ExpiresInvalidReason  = "Date is in the past";
                return;
            }

            if (_expiresTime == null)
            {
                if (_expiresDate != null)
                {
                    IsExpiresValid        = false;
                    _newWhitelist.Expires = null;
                    ExpiresInvalidReason  = "Date is set but time is not";
                    return;
                }

                IsExpiresValid        = null;
                _newWhitelist.Expires = null;
                return;
            }

            DateTime composite = _expiresDate.Value.Add(_expiresTime.Value);

            if (composite < DateTime.Now)
            {
                IsExpiresValid        = false;
                _newWhitelist.Expires = null;
                ExpiresInvalidReason  = "Time is in the past";
            }
            else
            {
                IsExpiresValid        = true;
                _newWhitelist.Expires = composite;
            }
        }

        public void UpdateIPList(List<string> ips)
        {
            if (ips.Count == 0)
            {
                _newWhitelist.IPs = null;
                return;
            }

            _newWhitelist.IPs = new List<IPAddress>();
            foreach (string ip in ips)
                if (ip != "" && ValidateIP(ip).Item1 == Validation.Valid)
                    _newWhitelist.IPs.Add(IPAddress.Parse(ip));

            OnValidation.Invoke();
        }

        public void UpdateHostnameList(List<string> hostnames)
        {
            if (hostnames.Count == 0)
            {
                _newWhitelist.Hostnames = null;
                return;
            }

            _newWhitelist.Hostnames = new List<string>();
            foreach (string hostname in hostnames)
                if (hostname != "" && ValidateHostname(hostname).Item1 != Validation.Invalid)
                    _newWhitelist.Hostnames.Add(hostname);

            OnValidation.Invoke();
        }

        public void UpdateSubnetList(List<string> subnets)
        {
            if (subnets.Count == 0)
            {
                _newWhitelist.Subnets = null;
                return;
            }

            _newWhitelist.Subnets = new List<IPNetwork>();
            foreach (string subnet in subnets)
                if (subnet != "")
                    _newWhitelist.Subnets.Add(IPNetwork.Parse(subnet));
            
            OnValidation.Invoke();
        }

        public void UpdateVendorList(List<string> vendors)
        {
            if (vendors.Count == 0)
            {
                _newWhitelist.Vendors = null;
                return;
            }

            _newWhitelist.Vendors = new List<string>();
            foreach (string vendor in vendors)
                if (vendor != "")
                    _newWhitelist.Vendors.Add(vendor);
            
            OnValidation.Invoke();
        }

        public bool CanCommit()
        {
            if (IsExpiresValid != null && IsExpiresValid == false)
                return false;

            if (!IsPatternValid)
                return false;

            bool a = _newWhitelist.IPs?.Any()       == true;
            bool b = _newWhitelist.Subnets?.Any()   == true;
            bool c = _newWhitelist.Hostnames?.Any() == true;
            bool d = _newWhitelist.MACs?.Any()      == true;
            bool e = _newWhitelist.Vendors?.Any()   == true;

            return a || b || c || d || e;
        }

        public void Commit()
        {
            if (!CanCommit()) return;

            // _newWhitelist.Expires   = IsExpiresValid == true ? _expiresDateAndTime : null;
            // _newWhitelist.IPs       = _ips.Any() ? _ips.Select(IPAddress.Parse).ToList() : null;
            // _newWhitelist.Subnets   = _subnets.Any() ? _subnets.Select(IPNetwork.Parse).ToList() : null;
            // _newWhitelist.Hostnames = _hostnames.Any() ? _hostnames : null;
            // _newWhitelist.MACs      = _macs.Any() ? _macs.Select(PhysicalAddress.Parse).ToList() : null;
            // _newWhitelist.Vendors   = _vendors.Any() ? _vendors : null;

            WhitelistModel.Submit(_newWhitelist);
            
            _newWhitelist = new Whitelist();
        }

        public (Validation, MarkupString) ValidateIP(string s)
        {
            if (s.Length == 0)
                return (Validation.Undefined, new MarkupString(""));

            if (_matchIPv4Regex.IsMatch(s))
                return (Validation.Valid, new MarkupString("Valid IP"));

            if (_matchIPv6Regex.IsMatch(s))
                return (Validation.Valid, new MarkupString("Valid IP"));

            return (Validation.Invalid, new MarkupString("Invalid IP"));
        }

        public (Validation, MarkupString) ValidateSubnet(string s)
        {
            if (s.Length == 0)
                return (Validation.Undefined, new MarkupString(""));

            bool valid = IPNetwork.TryParse(s, out IPNetwork parsed);

            if (valid)
                return (Validation.Valid, new MarkupString($"Parsed: {parsed}"));

            return (Validation.Invalid, new MarkupString("Parse failure"));
        }

        public (Validation, MarkupString) ValidateHostname(string s)
        {
            if (s.Length == 0)
                return (Validation.Undefined, new MarkupString(""));

            if (_validHostnameRegex.IsMatch(s))
                return (Validation.Valid, new MarkupString("Valid hostname"));

            return (Validation.Warning, new MarkupString("Invalid hostname"));
        }
    }
}