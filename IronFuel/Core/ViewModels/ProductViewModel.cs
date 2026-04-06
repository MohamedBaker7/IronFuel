using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public CategoryViewModel? Category { get; set; }
        public BrandViewModel? Brand { get; set; }
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        [Display(Name = "Flavour")]
        public string SelectedFlavour { get; set; } = string.Empty;
        public IEnumerable<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();
        public IEnumerable<SelectListItem> Flavors { get; set; } = new List<SelectListItem>();


        [Display(Name = "Sizes")]
        public string SelectedSizes { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Sizes { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Gallery images, ordered for display (primary first).
        /// </summary>
        public IReadOnlyList<ProductImageViewModel> Images { get; set; } = Array.Empty<ProductImageViewModel>();

        public decimal? LowestPrice
        {
            get
            {
                decimal? lowestPrice = Variants
                    .Select(v => v.Price)
                    .Order().FirstOrDefault();


                return lowestPrice;
            }
        }
    }
}
