using Microsoft.AspNetCore.Mvc.Rendering;

namespace IronFuel.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Products
            CreateMap<ProductImage, ProductImageViewModel>();
            CreateMap<Product, ProductViewModel>()
                .ForMember(d => d.Images, opt => opt.MapFrom(s =>
                    s.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList()));


            CreateMap<Flavour, SelectListItem>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id));

            // Brands
            CreateMap<Brand, BrandViewModel>();
            // Categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Category, CategoryNavViewModel>();
            // Variants
            CreateMap<ProductVariant, ProductVariantViewModel>();
        }
    }
}
