namespace IronFuel.Core.ViewModels
{
    public class BrandViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    }
}
