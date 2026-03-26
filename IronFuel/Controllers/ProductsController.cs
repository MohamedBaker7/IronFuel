using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.WebRequestMethods;

namespace IronFuel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private int? _categoryId;

        public ProductsController(IMapper mapper, ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public IActionResult Index(int? categoryId, string? categoryName)
        {
            if (categoryId is not null)
                _categoryId = categoryId;

            var products = GetProducts(_categoryId);


            var ProductsModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);

            return View(ProductPageViewModel(ProductsModel, categoryId, categoryName));
        }

        [AjaxOnly, HttpPost]
        public IActionResult Filter(ProductFilterViewModel filter)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var products = GetProducts(filter.CategoryId);

            var ProductsModel = _mapper.Map<IEnumerable<ProductViewModel>>(GetFilteredPrducts(products, filter));

            return PartialView("_ProductsCard", ProductsModel);
        }

        public IActionResult Details(int id)
        {

            var product = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.Variants)
                    .SingleOrDefault(p => p.Id == id);

            if(product is null)
                return NotFound();

            var viewModel = _mapper.Map<ProductViewModel>(product);

            viewModel.Flavors = product.Variants.Select(v => v.Flavour)
                    .Distinct()
                    .Select(f => new SelectListItem
                    {
                        Value = f,
                        Text = f
                    });

            return View(viewModel);
        }

        private List<Product> GetProducts(int? CategoryId)
        {
            var cacheKey = CategoryId is not null ? $"product_{CategoryId}" : "product_all";

            List<Product> products = new List<Product>();

            if (!_cache.TryGetValue(cacheKey, out products!))
            {
                products = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.Variants)
                    .Where(p => !p.IsDeleted && (p.CategoryId == CategoryId || CategoryId == null))
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, products, cacheOptions);
            }
            return products;
        }

        private IEnumerable<Product> GetFilteredPrducts(List<Product> products,ProductFilterViewModel filter)
        {
            if (filter.InStockOnly)
                products = products.Where(p => p.Variants.Any(v => v.StockQuantity > 0)).ToList();

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
            {
                if (filter.MinPrice >= filter.MaxPrice)
                    filter.MaxPrice = filter.MinPrice + 1;
            }

            if (filter.MinPrice.HasValue)
                products = products.Where(p => p.Variants.Any(v => v.Price >= filter.MinPrice)).ToList();


            if (filter.MaxPrice.HasValue)
                products = products.Where(p => p.Variants.Any(v => v.Price <= filter.MaxPrice)).ToList();

            if (filter.Flavors?.Length > 0)
            {
                products = products.Where(p => p.Variants.Any(v => filter.Flavors.Contains(v.Flavour))).ToList();
            }

            if (filter.Sizes?.Length > 0)
            {
                var parsedRanges = filter.Sizes
                        .Select(s =>
                        {
                            var parts = s.Split('-');

                            if (parts.Length != 2)
                                return null;

                            bool isMinValid = decimal.TryParse(parts[0], out decimal min);
                            bool isMaxValid = decimal.TryParse(parts[1], out decimal max);

                            if (!isMinValid || !isMaxValid)
                                return null;

                            return new
                            {
                                Min = min,
                                Max = max
                            };
                        })
                        .ToList();

                if (!parsedRanges.Any(p => p == null))
                {
                    products = products.Where(p => p.Variants.Any(v =>
                                    parsedRanges.Any(r => v.Size >= r?.Min && v.Size < r?.Max)))
                                    .ToList();
                }

            }

            return products;
        }

        private ProductPageViewModel ProductPageViewModel(IEnumerable<ProductViewModel> ProductsModel, int? categoryId, string? categoryName)
        {
            var ranges = new List<(decimal Min, decimal Max)>
                {
                    (0.0m, 0.5m),
                    (0.5m, 1m),
                    (1m, 3m),
                    (3m, 6m),
                    (6m, 12m)
                };
            ProductPageViewModel viewModel = new()
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                Products = ProductsModel,
                MaxPrice = ProductsModel.SelectMany(p => p.Variants.Select(v => v.Price)).Order().LastOrDefault(),

                Flavours = ProductsModel.SelectMany(p => p.Variants)
                            .GroupBy(p => p.Flavour)
                            .Select(g => new FlavourDto
                            {
                                Flavour = g.Key,
                                Count = g.Count()
                            })
                            .ToList(),




                Sizes = ranges
                        .Select(r => new SizeDto
                        {
                            Min = r.Min,
                            Max = r.Max,
                            Label = $"{r.Min} - {r.Max}",
                            Count = ProductsModel
                                .SelectMany(p => p.Variants, (product, variant) => new
                                {
                                    product.Id,
                                    variant.Size
                                })
                                .Where(x => x.Size >= r.Min && x.Size < r.Max)
                                .Select(x => x.Id)
                                .Distinct()
                                .Count()
                        })
                        .Where(x => x.Count > 0)
                        .ToList()
            };

            return viewModel;
        }
    }
}
