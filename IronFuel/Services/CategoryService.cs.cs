using IronFuel.Application.Common.Interfaces;

namespace IronFuel.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApplicationDbContext _context;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;

        public CategoryService(IApplicationDbContext context, CacheService cacheService, IMapper mapper)
        {
            _context = context;
            _cacheService = cacheService;
            _mapper = mapper;
        }


        public async Task<IReadOnlyList<CategoryViewModel>?> GetCategories()
        {
            var cacheKey = "categories_list";

            try
            {
                var cached = await _cacheService.GetAsync<IReadOnlyList<Category>>(cacheKey);
                if (cached is not null)
                    return _mapper.Map<IReadOnlyList<CategoryViewModel>>(cached);

            }
            catch
            {
                throw new Exception("Error retrieving categories from cache.");
            }
            var categories = await _context.Categories.ToListAsync();
            if (categories is null) return null;

            await _cacheService.SetAsync(cacheKey, categories, TimeSpan.FromHours(1));

            
            return _mapper.Map<IReadOnlyList<CategoryViewModel>>(categories);
        }

        public CategoryFormViewModel? BuildEditModel(int id)
        {
            var category = _context.Categories.Find(id);
            return category is null ? null :_mapper.Map<CategoryFormViewModel>(category);
        }

        public CategoryViewModel Create(CategoryFormViewModel model)
        {
            var category = _mapper.Map<Category>(model);
            //category.CreatedById = User.GetUserId();
            _context.Categories.Add(category);
            _context.SaveChanges();

            InvalidateCache();
            return _mapper.Map<CategoryViewModel>(category);

        }

        public CategoryViewModel? Update(CategoryFormViewModel model)
        {
            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return null;

            category = _mapper.Map(model, category);
            //category.LastUpdatedById = User.GetUserId();
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            InvalidateCache(); // Invalidate cache after update
            return _mapper.Map<CategoryViewModel>(category);

        }
        public (bool success, string? lastUpdateOn) toggleStatus(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return (false, null);

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();

            InvalidateCache();
            return (true, category.LastUpdatedOn?.ToString("MMM dd, yyyy"));
        }
        public bool AllowedItem(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);
            return category is null || category.Id.Equals(model.Id);
        }

        private async void InvalidateCache() => await _cacheService.RemoveAsync("categories_list");


    }
}
