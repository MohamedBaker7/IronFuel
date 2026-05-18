namespace IronFuel.Web.Services
{
    public interface IFlavorService
    {
        Task<IReadOnlyList<FlavorViewModel>?> GetFlavors();
        FlavorFormViewModel? BuildEditModel(int id);
        FlavorViewModel Create(FlavorFormViewModel model);
        FlavorViewModel? Update(FlavorFormViewModel model);
        (bool success, string? lastUpdateOn) ToggleStatus(int id);
        bool AllowedItem(FlavorFormViewModel model);
    }
}
