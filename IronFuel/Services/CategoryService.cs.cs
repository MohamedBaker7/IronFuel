using IronFuel.Application.Common.Interfaces;

namespace IronFuel.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public IReadOnlyList<CategoryViewModel> GetCategories()
        {
            var categories = _context.Categories.ToList();
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

            return _mapper.Map<CategoryViewModel>(category);

        }

        public CategoryViewModel? Update(CategoryFormViewModel model)
        {
            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return null;

            category = _mapper.Map(model, category); // Add Mapper In Edit Mode
            //category.LastUpdatedById = User.GetUserId();
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

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
            return (true, category.LastUpdatedOn?.ToString("MMM dd, yyyy"));
        }
        public bool AllowedItem(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);
            return category is null || category.Id.Equals(model.Id);
        }


    }
}
