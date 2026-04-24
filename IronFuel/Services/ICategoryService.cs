namespace IronFuel.Web.Services
{
    public interface ICategoryService
    {
        IReadOnlyList<CategoryViewModel> GetCategories();
        CategoryFormViewModel? BuildEditModel(int id);

        CategoryViewModel Create(CategoryFormViewModel model);

        CategoryViewModel? Update(CategoryFormViewModel model);


        (bool success, string? lastUpdateOn) toggleStatus(int id);

        bool AllowedItem(CategoryFormViewModel model);
    }
}
