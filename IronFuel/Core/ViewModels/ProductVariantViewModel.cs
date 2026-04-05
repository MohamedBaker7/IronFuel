namespace IronFuel.Web.Core.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductViewModel Product { get; set; } = null!;
        public int FlavourId { get; set; }
        public Flavour Flavour { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Size { get; set; }
        public int StockQuantity { get; set; }
    }
}
