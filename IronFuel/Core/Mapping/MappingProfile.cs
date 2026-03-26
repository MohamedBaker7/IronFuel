namespace IronFuel.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Products
            CreateMap<Product, ProductViewModel>();
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
