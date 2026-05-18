using IronFuel.Application.Common.Interfaces;

namespace IronFuel.Web.Services
{
    public class BrandService : IBrandService
    {
        private readonly IApplicationDbContext _context;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;

        public BrandService(IApplicationDbContext context, CacheService cacheService, IMapper mapper)
        {
            _context = context;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<BrandViewModel>?> GetBrands()
        {
            var cacheKey = "brands_list";

            try
            {
                var cached = await _cacheService.GetAsync<IReadOnlyList<Brand>>(cacheKey);
                if (cached is not null)
                    return _mapper.Map<IReadOnlyList<BrandViewModel>>(cached);
            }
            catch
            {
                throw new Exception("Error retrieving brands from cache.");
            }

            var brands = await _context.Brands.ToListAsync();
            if (brands is null) return null;

            await _cacheService.SetAsync(cacheKey, brands, TimeSpan.FromHours(1));

            return _mapper.Map<IReadOnlyList<BrandViewModel>>(brands);
        }

        public BrandFormViewModel? BuildEditModel(int id)
        {
            var brand = _context.Brands.Find(id);
            return brand is null ? null : _mapper.Map<BrandFormViewModel>(brand);
        }

        public BrandViewModel Create(BrandFormViewModel model)
        {
            var brand = _mapper.Map<Brand>(model);
            _context.Brands.Add(brand);
            _context.SaveChanges();
            return _mapper.Map<BrandViewModel>(brand);
        }

        public BrandViewModel? Update(BrandFormViewModel model)
        {
            var brand = _context.Brands.Find(model.Id);
            if (brand is null)
                return null;

            brand = _mapper.Map(model, brand);
            brand.LastUpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();

            return _mapper.Map<BrandViewModel>(brand);
        }

        public (bool success, string? lastUpdateOn) ToggleStatus(int id)
        {
            var brand = _context.Brands.Find(id);
            if (brand is null)
                return (false, null);

            brand.IsDeleted = !brand.IsDeleted;
            brand.LastUpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();
            return (true, brand.LastUpdatedOn?.ToString("MMM dd, yyyy"));
        }

        public bool AllowedItem(BrandFormViewModel model)
        {
            var brand = _context.Brands.SingleOrDefault(b => b.Name == model.Name);
            return brand is null || brand.Id.Equals(model.Id);
        }
    }
}
