using System.Runtime.Intrinsics.X86;

namespace IronFuel.Domain.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string Description { get; set; } = null!;
        public string? Benefits { get; set; } 
        public string? SuggestedUse { get; set; } 

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    }
}
