using IronFuel.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.CodeAnalysis.Elfie.Extensions;

namespace IronFuel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [EnableRateLimiting("fixed")]
    [Authorize(Roles = AppRoles.Admin)]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetManageProductsAsync();
            return View(products);
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetDataTableProducts()
        {
            var result = await _productService.GetDataTableProductsAsync(Request.Form);
            return Ok(new
            {
                draw = result.Draw,
                recordsTotal = result.RecordsTotal,
                recordsFiltered = result.RecordsFiltered,
                data = result.Data
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _productService.BuildCreateModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductEditorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var populated = await _productService.BuildCreateModelAsync();
                model.CategoryOptions = populated.CategoryOptions;
                model.BrandOptions = populated.BrandOptions;
                model.FlavourOptions = populated.FlavourOptions;
                return View(model);
            }

            var result = await _productService.CreateAsync(model);
            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorKey) && !string.IsNullOrWhiteSpace(result.ErrorMessage))
                    ModelState.AddModelError(result.ErrorKey, result.ErrorMessage);

                var populated = await _productService.BuildCreateModelAsync();
                model.CategoryOptions = populated.CategoryOptions;
                model.BrandOptions = populated.BrandOptions;
                model.FlavourOptions = populated.FlavourOptions;
                return View(model);
            }

            TempData["StatusMessage"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.BuildEditModelAsync(id);
            return model is null ? NotFound() : View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductEditorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var existing = await _productService.BuildEditModelAsync(id);
                if (existing is null)
                    return NotFound();

                model.CategoryOptions = existing.CategoryOptions;
                model.BrandOptions = existing.BrandOptions;
                model.FlavourOptions = existing.FlavourOptions;
                model.ExistingImages = existing.ExistingImages;
                return View(model);
            }

            var result = await _productService.UpdateAsync(id, model);
            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorKey) && !string.IsNullOrWhiteSpace(result.ErrorMessage))
                    ModelState.AddModelError(result.ErrorKey, result.ErrorMessage);

                var existing = await _productService.BuildEditModelAsync(id);
                if (existing is null)
                    return NotFound();

                model.CategoryOptions = existing.CategoryOptions;
                model.BrandOptions = existing.BrandOptions;
                model.FlavourOptions = existing.FlavourOptions;
                model.ExistingImages = existing.ExistingImages;
                return View(model);
            }

            TempData["StatusMessage"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var updated = await _productService.ToggleStatusAsync(id);
            if (!updated)
                return NotFound();

            TempData["StatusMessage"] = "Product status updated.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AllowedItem(ProductEditorViewModel model)
        {
            var isAllowed = _productService.AllowedItem(model);
            return Json(isAllowed);
        }

    }
}
