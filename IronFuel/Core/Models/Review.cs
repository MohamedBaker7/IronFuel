namespace IronFuel.Core.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? UserId { get; set; }

        public int Rating { get; set; }

        public string Headline { get; set; } = null!;
        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }
}
