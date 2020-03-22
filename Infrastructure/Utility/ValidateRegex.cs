using System;
using System.Text.RegularExpressions;

namespace Infrastructure.Utility
{
    public static partial class Utility
    {
        // https://stackoverflow.com/a/1775017/9911189
        public static bool ValidateRegex(string regex)
        {
            if (string.IsNullOrEmpty(regex)) return false;
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.Match("", regex);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}