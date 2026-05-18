namespace IronFuel.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public DateTime OrderedOn { get; set; }
        public string Status { get; set; } = "Pending";
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = null!;
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
