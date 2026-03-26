using System.ComponentModel.DataAnnotations;

namespace IronFuel.Core.Models
{
    public class Category : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
