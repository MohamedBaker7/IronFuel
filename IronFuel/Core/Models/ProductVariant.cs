using System.ComponentModel.DataAnnotations;

namespace IronFuel.Core.Models
{
    public class ProductVariant : BaseModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        [MaxLength(200)]
        public string Flavour { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Size { get; set; }
        public int StockQuantity { get; set; }

    }
}
