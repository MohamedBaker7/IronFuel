namespace IronFuel.Web.Services
{
    public interface IFlavorService
    {
        Task<IReadOnlyList<FlavorViewModel>?> GetFlavors();
        FlavorFormViewModel? BuildEditModel(int id);
        FlavorViewModel Create(FlavorFormViewModel model);
        FlavorViewModel? Update(FlavorFormViewModel model);
        (bool success, string? lastUpdateOn) ToggleStatus(int id);
        bool AllowedName(FlavorFormViewModel model);
        bool AllowedCode(FlavorFormViewModel model);
        bool IsCodeUsedInSKU(FlavorFormViewModel model);
    }
}
