namespace IronFuel.Web.Core.ViewModels
{
    public class ProductsPageViewModel
    {
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? MaxPrice { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<ProductFlavourFacetDto> Flavours { get; set; } = new List<ProductFlavourFacetDto>();
        public List<ProductSizeFacetDto> Sizes { get; set; } = new List<ProductSizeFacetDto>();

    }
}
