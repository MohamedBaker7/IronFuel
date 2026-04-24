namespace IronFuel.Web.Services
{
    public interface IBrandService
    {
        IReadOnlyList<BrandViewModel> GetBrands();
        BrandFormViewModel? BuildEditModel(int id);
        BrandViewModel Create(BrandFormViewModel model);
        BrandViewModel? Update(BrandFormViewModel model);
        (bool success, string? lastUpdateOn) ToggleStatus(int id);
        bool AllowedItem(BrandFormViewModel model);
    }
}
