using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace IronFuel.Web.Services
{
    public interface IProductService
    {
        ProductsPageViewModel GetProductsPage(int? categoryId, string? categoryName);
        FilterResultDto GetFilteredProducts(ProductFilterDto filter);
        Task<ProductViewModel?> GetProductDetailsAsync(int id);
        Task<IReadOnlyList<ProductViewModel>> GetManageProductsAsync();
        Task<DataTableResult<ProductViewModel>> GetDataTableProductsAsync(IFormCollection form);
        Task<ProductEditorViewModel> BuildCreateModelAsync();
        Task<ProductEditorViewModel?> BuildEditModelAsync(int id);
        Task<(bool Success, string? ErrorKey, string? ErrorMessage)> CreateAsync(ProductEditorViewModel model);
        Task<(bool Success, string? ErrorKey, string? ErrorMessage)> UpdateAsync(int id, ProductEditorViewModel model);
        Task<bool> ToggleStatusAsync(int id);
        Task<(IReadOnlyList<object> Sizes, decimal Price)> GetVariantSelectionDataAsync(int productId, int flavourId, decimal? weightG);
    
        bool AllowedItem(ProductEditorViewModel model);
    }

    public class DataTableResult<T>
    {
        public string? Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
    }
}
