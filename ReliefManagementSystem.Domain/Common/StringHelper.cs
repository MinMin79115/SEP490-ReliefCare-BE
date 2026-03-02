using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Common
{
    public static class StringHelper
    {
        public static string NormalizeVietnamese(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = text.Replace('Đ', 'D').Replace('đ', 'd');

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string NormalizeVietnamesePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Replace('Đ', 'D').Replace('đ', 'd');

            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC);

            // lowercase
            result = result.ToLowerInvariant();

            // remove special chars
            result = Regex.Replace(result, @"[^a-z0-9\s-]", "");

            // spaces → dash
            result = Regex.Replace(result, @"\s+", "-");

            // trim dash
            result = result.Trim('-');

            return result;
        }
    }
}
