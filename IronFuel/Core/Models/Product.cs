using System.ComponentModel.DataAnnotations;

namespace IronFuel.Core.Models
{
    public class Product : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }       
        public string Description { get; set; } = null!;


        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    }
}
