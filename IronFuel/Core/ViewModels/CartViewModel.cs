namespace IronFuel.Web.Core.ViewModels
{
    public class CartViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public IReadOnlyList<CartItemViewModel> Items { get; set; } = Array.Empty<CartItemViewModel>();
        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
