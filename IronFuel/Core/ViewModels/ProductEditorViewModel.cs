using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace IronFuel.Web.Core.ViewModels
{
    public class ProductEditorViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Name"), MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Remote(action: "AllowedItem", controller: "Products", areaName: "Admin", AdditionalFields = "Id,BrandId", ErrorMessage = Errors.DuplicatedProducts)]
        public string Name { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a brand.")]
        [Display(Name = "Brand")]
        [Remote(action: "AllowedItem", controller: "Products", areaName: "Admin", AdditionalFields = "Id,Name", ErrorMessage = Errors.DuplicatedProducts)]
        public int BrandId { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Display(Name = "Benefits")]
        public string? Benefits { get; set; }

        [Display(Name = "Suggested use")]
        public string? SuggestedUse { get; set; }

        /// <summary>Stored relative URL (e.g. ~/Videos/productVideos/{id}/file.mp4). Round-tripped on edit.</summary>
        [Display(Name = "Product video URL")]
        [MaxLength(2048, ErrorMessage = "URL must not exceed 2048 characters.")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Product video")]
        public IFormFile? ProductVideo { get; set; }

        [Display(Name = "Remove current video")]
        public bool RemoveProductVideo { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one product variant.")]
        public IList<ProductVariantInputViewModel> Variants { get; set; } = new List<ProductVariantInputViewModel>
        {
            new()
        };

        [Display(Name = "Gallery images")]
        [RequiredIf("Id == 0", ErrorMessage = "Gallery images are required.")]
        public IList<IFormFile>? GalleryImages { get; set; } = new List<IFormFile>();
        public IList<ProductImageEditorViewModel> ExistingImages { get; set; } = new List<ProductImageEditorViewModel>();
        public string? RemovedImageIds { get; set; }
        public string? ExistingImageOrder { get; set; }

        public IEnumerable<SelectListItem> FlavourOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BrandOptions { get; set; } = new List<SelectListItem>();
    }

    public class ProductImageEditorViewModel
    {
        public int Id { get; set; }
        public string RelativePath { get; set; } = null!;
        public int SortOrder { get; set; }
    }

    public class ProductVariantInputViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Select a flavour.")]
        [Display(Name = "Flavour")]
        public int FlavourId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
        [Display(Name = "Weight (g)")]
        public int WeightG { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serving size must be greater than zero.")]
        [Display(Name = "Serving size (g)")]
        public int ServingSizeG { get; set; }

        [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "Price must be greater than zero.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }
    }
}
