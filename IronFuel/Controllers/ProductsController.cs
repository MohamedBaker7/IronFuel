using IronFuel.Web.Services;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

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

        [AjaxOnly,HttpPost]
        public IActionResult Filter(ProductFilterDto filter)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = _productService.GetFilteredProducts(filter);

            return Json(new
            {
                html = this.RenderPartialViewToString("_ProductsCard", result.Products),
                result.AvailableFlavors,
                result.AvailableSizes,
                result.TotalCount
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            return product is null ? NotFound() : View(product);
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> GetSizesByFlavour(int productId, int flavourId)
        {
            var data = await _productService.GetSizesSelectionDataAsync(productId, flavourId);
            return Json(data);
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> GetProductSKU(int productId, int flavourId, int weightG)
        {
            var sku = await _productService.GetSKU(productId, flavourId, weightG);
            return Json(sku);
        }

        [HttpGet, AjaxOnly]
        public async Task<IActionResult> GetProductPrice(string SKU)
        {
            var price = await _productService.GetPrice(SKU);
            return Json(price);
        }
    }
}
