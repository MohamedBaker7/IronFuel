using System.ComponentModel.DataAnnotations;

namespace IronFuel.Application.DTO.Products
{
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public bool InStockOnly { get; set; }

        [Range(0, 100000)]
        public decimal? MinPrice { get; set; }

        [Range(0, 100000)]
        public decimal? MaxPrice { get; set; }
        public string[]? Flavors { get; set; }
        public string[]? Sizes { get; set; }
    }
}
