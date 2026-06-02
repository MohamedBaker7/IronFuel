using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IronFuel.Infrastructure.Persistence.Converters
{
    public class PascalCaseConverter : ValueConverter<string, string>
    {
        public PascalCaseConverter() : base(v => ToPascalCase(v), v => v) { }

        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(value.Trim().ToLower());
        }
    }
}
