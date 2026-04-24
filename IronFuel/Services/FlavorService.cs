using IronFuel.Application.Common.Interfaces;

namespace IronFuel.Web.Services
{
    public class FlavorService : IFlavorService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FlavorService(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IReadOnlyList<FlavorViewModel> GetFlavors()
        {
            var flavors = _context.Flavors.ToList();
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
