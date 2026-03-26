using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IronFuel.Core.ViewModels
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

        public IEnumerable<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();

        [Display(Name = "Flavour")]
        public string SelectedFlavour { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Flavors { get; set; } = new List<SelectListItem>();

        public decimal? LowestPrice { 
            get {
                decimal? lowestPrice = Variants
                    .Select(v => v.Price)
                    .Order().FirstOrDefault();


                return lowestPrice;
            }
        }
    }
}
