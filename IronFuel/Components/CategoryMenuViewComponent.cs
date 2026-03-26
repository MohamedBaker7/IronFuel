namespace IronFuel.Components
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CategoryMenuViewComponent(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<CategoryNavViewModel>>(categories);

            return View(viewModel);
        }

    }
}
