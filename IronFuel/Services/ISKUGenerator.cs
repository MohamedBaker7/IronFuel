namespace IronFuel.Web.Services
{
    public interface ISKUGenerator
    {
        string Generate(string brandCode, string productCode, string flavourCode, int weightG);
    }
}
