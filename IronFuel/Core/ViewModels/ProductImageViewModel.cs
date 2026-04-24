namespace IronFuel.Web.Core.ViewModels
{
    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string RelativePath { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
