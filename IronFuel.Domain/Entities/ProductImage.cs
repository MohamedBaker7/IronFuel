namespace IronFuel.Domain.Entities
{
    /// <summary>
    /// Gallery image for a product. <see cref="RelativePath"/> is under wwwroot (e.g. <c>Images/productImages/1/photo.jpg</c>).
    /// </summary>
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [MaxLength(500)]
        public string RelativePath { get; set; } = null!;

        /// <summary>
        /// Lower values appear first; use 0 for the primary image.
        /// </summary>
        public int SortOrder { get; set; }
    }
}
