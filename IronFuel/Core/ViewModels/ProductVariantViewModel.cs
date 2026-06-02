namespace IronFuel.Web.Core.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductViewModel Product { get; set; } = null!;
        public int FlavourId { get; set; }
        public Flavour Flavour { get; set; } = null!;
        public int WeightG { get; set; }
        public int ServingSizeG { get; set; }
        public int ServingsPerContainer { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; } = null!;
    }
}
