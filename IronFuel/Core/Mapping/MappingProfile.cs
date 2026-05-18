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
            CreateMap<Brand, BrandViewModel>().ReverseMap();
            CreateMap<Brand, BrandFormViewModel>().ReverseMap();

            // Flavors
            CreateMap<Flavour, FlavorViewModel>().ReverseMap();
            CreateMap<Flavour, FlavorFormViewModel>().ReverseMap();
            // Categories
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<Category, CategoryFormViewModel>().ReverseMap();
            CreateMap<Category, CategoryNavViewModel>();
            // Variants
            CreateMap<ProductVariant, ProductVariantViewModel>();

            // Cart & Orders
            CreateMap<Cart, CartViewModel>();
            CreateMap<CartItem, CartItemViewModel>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.ProductVariant.Product.Name))
                .ForMember(d => d.FlavourName, opt => opt.MapFrom(s => s.ProductVariant.Flavour.Name))
                .ForMember(d => d.WeightG, opt => opt.MapFrom(s => s.ProductVariant.WeightG));

            CreateMap<Order, OrderViewModel>();
            CreateMap<OrderItem, OrderItemViewModel>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.ProductVariant.Product.Name))
                .ForMember(d => d.FlavourName, opt => opt.MapFrom(s => s.ProductVariant.Flavour.Name))
                .ForMember(d => d.WeightG, opt => opt.MapFrom(s => s.ProductVariant.WeightG));
        }
    }
}
