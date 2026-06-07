namespace IronFuel.Web.Core.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string FlavourName { get; set; } = string.Empty;
        public int WeightG { get; set; }
        public int Servings { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
