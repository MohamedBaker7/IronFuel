using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public CategoryViewModel? Category { get; set; }
        public int BrandId { get; set; }
        public BrandViewModel? Brand { get; set; }
        public string Description { get; set; } = null!;
        public string? Benefits { get; set; }
        public string? SuggestedUse { get; set; }

        public string? VideoUrl { get; set; }

        public bool IsDeleted { get; set; }

        [Display(Name = "Flavour")]
        public string SelectedFlavour { get; set; } = string.Empty;
        public IEnumerable<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();
        public IEnumerable<SelectListItem> Flavors { get; set; } = new List<SelectListItem>();


        [Display(Name = "Sizes")]
        public string SelectedSizes { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Sizes { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<ProductImageViewModel> Images { get; set; } = Array.Empty<ProductImageViewModel>();


        public string? PrimaryImage
        {
            get
            {
                var primary = Images
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefault()?
                    .RelativePath;

                return primary is null ? string.Empty : primary;
            }
        }
        public string? SecondaryImage
        {
            get
            {

                var secondary = Images
                    .OrderBy(i => i.SortOrder)
                    .Skip(1)
                    .Take(1)
                    .FirstOrDefault()?
                    .RelativePath;

                return secondary is null ? string.Empty : secondary;
            }
        }
        public decimal? LowestPrice
        {
            get
            {
                decimal? lowestPrice = Variants
                    .OrderBy(v => v.Price)
                    .FirstOrDefault()?
                    .Price;


                return lowestPrice;
            }
        }
    }
}
