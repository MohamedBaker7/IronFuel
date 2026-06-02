using IronFuel.Application.Common.Interfaces;
using Microsoft.DotNet.Scaffolding.Shared.Project;

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
            
            if(brand is null)
                return null;

            var viewModel = _mapper.Map<BrandFormViewModel>(brand);

            viewModel.IsCodeLocked = _context.ProductVariants
            .Any(v => v.SKU.Contains(brand.Code + "-"));

            return viewModel;
        }

        public BrandViewModel Create(BrandFormViewModel model)
        {
            var brand = _mapper.Map<Brand>(model);
            _context.Brands.Add(brand);
            _context.SaveChanges();

            InvalidateCache();
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

            InvalidateCache();
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

            InvalidateCache();
            return (true, brand.LastUpdatedOn?.ToString("MMM dd, yyyy"));
        }

        public bool AllowedName(BrandFormViewModel model)
        {
            var brand = _context.Brands.SingleOrDefault(b => b.Name == model.Name);
            return brand is null || brand.Id.Equals(model.Id);
        }

        public bool AllowedCode(BrandFormViewModel model)
        {
            var brand = _context.Brands.SingleOrDefault(b => b.Code == model.Code);

            return brand is null || brand.Id.Equals(model.Id);
        }

        public bool IsCodeUsedInSKU(BrandFormViewModel model)
        {
            var brand = _context.Brands.Find(model.Id);

            if (brand is null || brand.Code == model.Code.ToUpper())
                return false; 

            return _context.ProductVariants
                .Any(v => v.SKU.StartsWith(brand.Code + "-"));
        }

        private async void InvalidateCache() => await _cacheService.RemoveAsync("brands_list");
    }
}
