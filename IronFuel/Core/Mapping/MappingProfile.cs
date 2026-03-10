namespace IronFuel.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Categories
            CreateMap<Product, ProductViewModel>();
        }
    }
}
