using IronFuel.Web.Core.DTO;

namespace IronFuel.Web.Core.ViewModels
{
    public class CartViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public Guid CartToken { get; set; }
        public CartStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public IReadOnlyList<CartItemViewModel> Items { get; set; } = Array.Empty<CartItemViewModel>();
        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
