using IronFuel.Core.ViewModels;
using IronFuel.Data;
using Microsoft.AspNetCore.Mvc;

namespace IronFuel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            var products = _context.Products.Where(p => !p.IsDeleted).ToList();

            var viewModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);


            return View(viewModel);
        }
    }
}
