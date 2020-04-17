using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Infrastructure.Utility
{
    public static partial class Utility
    {
        // https://stackoverflow.com/a/4260512/9911189
        private static readonly Regex _validateMACRegex =
            new Regex(
                @"^([0-9A-Fa-f]{2}){6}$",
                RegexOptions.Compiled);

        public static bool ValidateMAC(string s)
        {
            s = s.ToUpper();
            s = s.Replace("-", "");
            s = s.Replace(":", "");

            try
            {
                if (_validateMACRegex.IsMatch(s))
                {
                    PhysicalAddress result = PhysicalAddress.Parse(s);
                    if (!result.Equals(PhysicalAddress.None))
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public static string FormatMAC(string s)
        {
            s = s.ToUpper();
            s = s.Replace("-", "");
            s = s.Replace(":", "");

            return string.Join(":",
                (from z in PhysicalAddress.Parse(s).GetAddressBytes() select z.ToString("X2")).ToArray());
        }

        public static string FormatMAC(PhysicalAddress mac)
        {
            return string.Join(":",
                (from z in mac.GetAddressBytes() select z.ToString("X2")).ToArray());
        }

        public static string CleanMAC(string s)
        {
            s = s.ToUpper();
            s = s.Replace(":", "");
            return s;
        }
    }
}