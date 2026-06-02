using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Infrastructure.Persistence.Converters
{
    public class UpperCaseConverter : ValueConverter<string, string>
    {
        public UpperCaseConverter() : base(v => v.Trim().ToUpper(), v => v) { }
    }
}
