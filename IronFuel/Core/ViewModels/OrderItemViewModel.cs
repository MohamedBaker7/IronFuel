namespace IronFuel.Web.Core.ViewModels
{
    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string FlavourName { get; set; } = string.Empty;
        public int WeightG { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
