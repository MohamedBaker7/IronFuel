namespace IronFuel.Web.Core.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderedOn { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public IReadOnlyList<OrderItemViewModel> Items { get; set; } = Array.Empty<OrderItemViewModel>();
    }
}
