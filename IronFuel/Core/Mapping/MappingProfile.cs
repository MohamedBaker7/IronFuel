using Microsoft.AspNetCore.Mvc.Rendering;

namespace IronFuel.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Products
            CreateMap<Product, ProductViewModel>();


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
