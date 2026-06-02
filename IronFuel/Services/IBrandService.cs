namespace IronFuel.Web.Services
{
    public interface IBrandService
    {
        Task<IReadOnlyList<BrandViewModel>?> GetBrands();
        BrandFormViewModel? BuildEditModel(int id);
        BrandViewModel Create(BrandFormViewModel model);
        BrandViewModel? Update(BrandFormViewModel model);
        (bool success, string? lastUpdateOn) ToggleStatus(int id);
        bool AllowedName(BrandFormViewModel model);
        bool AllowedCode(BrandFormViewModel model);

        bool IsCodeUsedInSKU(BrandFormViewModel model);
    }
}
