using IronFuel.Core.DTO;

namespace IronFuel.Core.ViewModels
{
    public class ProductPageViewModel
    {
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? MaxPrice { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<FlavourDto> Flavours { get; set; } = new List<FlavourDto>();
        public List<SizeDto> Sizes { get; set; } = new List<SizeDto>();

    }
}
