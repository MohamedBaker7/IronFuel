namespace IronFuel.Web.Core.DTO
{
    public class CreateCartDto
    {
        public string? UserId { get; set; }
    }

    public class AddToCartDto
    {
        
    }
    public class CartDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public Guid CartToken { get; set; }
        public CartStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
