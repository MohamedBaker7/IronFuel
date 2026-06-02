

namespace IronFuel.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public int Id { get; set; }
        public Guid CartToken { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public CartStatus Status { get; set; } = CartStatus.Active;
        public DateTime ExpiresAt { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
