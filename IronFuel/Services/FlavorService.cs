using IronFuel.Application.Common.Interfaces;

namespace IronFuel.Web.Services
{
    public class FlavorService : IFlavorService
    {
        private readonly IApplicationDbContext _context;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;

        public FlavorService(IApplicationDbContext context, CacheService cacheService, IMapper mapper)
        {
            _context = context;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FlavorViewModel>?> GetFlavors()
        {
            var cacheKey = "flavors_list";

            try
            {
                var cached = await _cacheService.GetAsync<IReadOnlyList<Flavour>>(cacheKey);
                if (cached is not null)
                    return _mapper.Map<IReadOnlyList<FlavorViewModel>>(cached);

            }
            catch
            {
                throw new Exception("Error retrieving flavors from cache.");
            }
            var flavors = await _context.Flavors.ToListAsync();
            if (flavors is null) return null;

            await _cacheService.SetAsync(cacheKey, flavors, TimeSpan.FromHours(1));

            return _mapper.Map<IReadOnlyList<FlavorViewModel>>(flavors);
        }

        public FlavorFormViewModel? BuildEditModel(int id)
        {
            var flavor = _context.Flavors.Find(id);
            return flavor is null ? null : _mapper.Map<FlavorFormViewModel>(flavor);
        }

        public FlavorViewModel Create(FlavorFormViewModel model)
        {
            var flavor = _mapper.Map<Flavour>(model);
            _context.Flavors.Add(flavor);
            _context.SaveChanges();
            return _mapper.Map<FlavorViewModel>(flavor);
        }

        public FlavorViewModel? Update(FlavorFormViewModel model)
        {
            var flavor = _context.Flavors.Find(model.Id);
            if (flavor is null)
                return null;

            flavor = _mapper.Map(model, flavor);
            flavor.LastUpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();

            return _mapper.Map<FlavorViewModel>(flavor);
        }

        public (bool success, string? lastUpdateOn) ToggleStatus(int id)
        {
            var flavor = _context.Flavors.Find(id);
            if (flavor is null)
                return (false, null);

            flavor.IsDeleted = !flavor.IsDeleted;
            flavor.LastUpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();
            return (true, flavor.LastUpdatedOn?.ToString("MMM dd, yyyy"));
        }

        public bool AllowedItem(FlavorFormViewModel model)
        {
            var flavor = _context.Flavors.SingleOrDefault(f => f.Name == model.Name);
            return flavor is null || flavor.Id.Equals(model.Id);
        }
    }
}
