using Server_API.Domain.Service.InfrastrutureService.Interface;
using System.Globalization;
using System.Text;

namespace Server_API.Domain.Service.InfrastrutureService
{
    public class NormalizeService : INormalizeService
    {
        public string NormalizeString(string text)
        {
            // Remover acentos e converter para minúsculas
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) !=
                                          UnicodeCategory.NonSpacingMark)
                             .ToArray();
            string normalizedText = new string(chars).Normalize(NormalizationForm.FormC).ToLowerInvariant();
            return normalizedText;
        }

        public string NormalizeStringSize(string text, int size)
        {
            string normalized = text;
            try
            {
                if (text.Length > size)
                {
                    normalized = text.Substring(0, size);
                }
                else
                {
                    normalized = text + new string(' ', size - text.Length);
                }
                return normalized;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public string NormalizeValue(string value)
        {
            string normalizedValue = value.Replace(".", ",")
                                     .Replace("-", "");
            return normalizedValue;
        }

        public decimal NormalizeToDecimal(string value)
        {
            if (decimal.TryParse(value.Replace(".", ",")
                                      .Replace("-", ""), out decimal decimalValue))
            {
                return decimalValue;
            }
            return 0.0m;
        }
    }
}