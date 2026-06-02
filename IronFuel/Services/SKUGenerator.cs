namespace IronFuel.Web.Services
{
    public class SKUGenerator : ISKUGenerator
    {
        public string Generate(string brandCode, string productCode, string flavourCode, int weightG)
        {
            var b = Sanitize(brandCode, 4);
            var p = Sanitize(productCode, 6);
            var f = Sanitize(flavourCode, 4);
            return $"{b}-{p}-{f}-{weightG}G";
            // e.g. "WHEY-CHOC-1000G"
        }

        private static string Sanitize(string code, int maxLength) =>
            code.ToUpper()
                .Replace(" ", "")
                .Replace("-", "")
                .Substring(0, Math.Min(maxLength, code.Length));
    }
}
