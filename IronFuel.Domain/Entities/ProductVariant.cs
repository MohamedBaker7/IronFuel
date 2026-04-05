namespace IronFuel.Domain.Entities
{
    [Index(nameof(ProductId), nameof(FlavourId), nameof(Size), IsUnique = true)]
    public class ProductVariant : BaseEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int FlavourId { get; set; }
        public Flavour Flavour { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Size { get; set; }
        public decimal ServingWeight { get; set; }
        public int StockQuantity { get; set; }

    }
}
