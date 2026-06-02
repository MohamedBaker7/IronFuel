namespace IronFuel.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        [MaxLength(10)]
        public string Code { get; set; } = null!; // we will convert this to required future
        public string? LogoUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
