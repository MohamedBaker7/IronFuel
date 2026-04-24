using IronFuel.Web.Services;

namespace IronFuel.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index(int? categoryId, string? categoryName)
        {
            var model = _productService.GetProductsPage(categoryId, categoryName);
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public IActionResult Filter(ProductFilterDto filter)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var products = _productService.GetFilteredProducts(filter);
            return PartialView("_ProductsCard", products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            return product is null ? NotFound() : View(product);
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> GetSizesByFlavour(int productId, int flavourId)
        {
            var data = await _productService.GetVariantSelectionDataAsync(productId, flavourId, null);
            return Json(data.Sizes);
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> GetProductPrice(int productId, int flavourId, decimal weightG)
        {
            var data = await _productService.GetVariantSelectionDataAsync(productId, flavourId, weightG);
            return Json(data.Price);
        }
    }
}
