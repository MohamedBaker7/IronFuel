namespace IronFuel.Web.Core.ViewModels
{
    public class ProductImageViewModel
    {
        /// <summary>
        /// Path relative to wwwroot (no leading slash), e.g. <c>Images/productImages/1/a.jpg</c>.
        /// </summary>
        public string RelativePath { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
