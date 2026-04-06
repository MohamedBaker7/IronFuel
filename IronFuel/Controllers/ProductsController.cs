using Microsoft.AspNetCore.Mvc.Rendering;

namespace IronFuel.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private int? _categoryId;
        private static readonly (decimal Min, decimal Max)[] _sizeRanges =
        {
            (0.0m, 0.5m),
            (0.5m, 1m),
            (1m, 3m),
            (3m, 6m),
            (6m, 12m)
        };

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

            return View(BuildProductPageViewModel(ProductsModel, categoryId, categoryName));
        }

        [AjaxOnly, HttpPost]
        public IActionResult Filter(ProductFilterDto filter)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var products = GetProducts(filter.CategoryId);

            var ProductsModel = _mapper.Map<IEnumerable<ProductViewModel>>(GetFilteredProducts(products, filter));

            return PartialView("_ProductsCard", ProductsModel);
        }

        public IActionResult Details(int id)
        {

            var product = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Variants)
                    .ThenInclude(v => v.Flavour)
                    .SingleOrDefault(p => p.Id == id);

            if (product is null)
                return NotFound();


            var viewModel = _mapper.Map<ProductViewModel>(product);

            viewModel.Flavors = product.Variants.Select(v => v.Flavour)
                    .Distinct()
                    .Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Name
                    });

            viewModel.Sizes = product.Variants
                    .Select(s => new { s.Size, s.ServingWeight })
                    .Distinct()
                    .OrderBy(s => s.Size)
                    .Select(f => new SelectListItem
                    {
                        Value = f.Size.ToString(),
                        Text = f.Size >= 1000 ? $"{(f.Size / 1000).ToString("0.##")} kg ({Math.Round(f.Size / f.ServingWeight)} Servings)"
                        : $"{f.Size.ToString("0.##")} g ({Math.Round(f.Size / f.ServingWeight)} Servings)"
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
                    .ThenInclude(v => v.Flavour)
                    .Where(p => !p.IsDeleted && (p.CategoryId == CategoryId || CategoryId == null))
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, products, cacheOptions);
            }
            return products;
        }

        [HttpGet]
        public IActionResult GetSizesByFlavour(int productId, int flavourId)
        {
            var sizes = _context.ProductVariants
                .Where(v => v.FlavourId == flavourId && v.ProductId == productId)
                .Select(v => new { v.Size, v.ServingWeight })
                .Distinct()
                .ToList();

            return Json(sizes);
        }

        private IEnumerable<Product> GetFilteredProducts(List<Product> products, ProductFilterDto filter)
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
                products = products.Where(p => p.Variants.Any(v => filter.Flavors.Contains(v.Flavour.Name))).ToList();
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
                                    parsedRanges.Any(r => v.Size >= r?.Min * 1000 && v.Size < r?.Max * 1000)))
                                    .ToList();
                }

            }

            return products;
        }

        private ProductPageViewModel BuildProductPageViewModel(IEnumerable<ProductViewModel> productsModel, int? categoryId, string? categoryName)
        {
            var products = productsModel.ToList();

            var productVariants = products
                .SelectMany(p => p.Variants.Select(v => new
                {
                    ProductId = p.Id,
                    FlavourName = v.Flavour.Name,
                    v.Price,
                    v.Size
                }))
                .ToList();

            ProductPageViewModel viewModel = new()
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                Products = products,
                MaxPrice = productVariants.Select(v => v.Price).Order().LastOrDefault(),
                Flavours = productVariants
                    .Select(v => new { v.ProductId, v.FlavourName })
                    .Distinct()
                    .GroupBy(x => x.FlavourName)
                    .Select(g => new ProductFlavourFacetDto
                    {
                        Flavour = g.Key,
                        Count = g.Count()
                    })
                    .ToList(),
                Sizes = _sizeRanges
                        .Select(r => new ProductSizeFacetDto
                        {
                            Min = r.Min,
                            Max = r.Max,
                            Label = $"{r.Min} - {r.Max}",
                            Count = productVariants
                                .Where(x => x.Size >= r.Min * 1000 && x.Size < r.Max * 1000)
                                .Select(x => x.ProductId)
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
