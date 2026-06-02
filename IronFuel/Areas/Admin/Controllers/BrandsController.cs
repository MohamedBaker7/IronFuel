using IronFuel.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace IronFuel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [EnableRateLimiting("fixed")]
    [Authorize(Roles = AppRoles.Admin)]
    public class BrandsController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetBrands();
            return View(brands);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public IActionResult Create(BrandFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var brandModel = _brandService.Create(model);
            return PartialView("_BrandRow", brandModel);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Edit(int id)
        {
            var model = _brandService.BuildEditModel(id);
            if (model is null)
                return NotFound();

            return PartialView("_Form", model);
        }

        [HttpPost]
        public IActionResult Edit(BrandFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var brandModel = _brandService.Update(model);
            if (brandModel is null)
                return NotFound();

            return PartialView("_BrandRow", brandModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var (success, lastUpdatedOn) = _brandService.ToggleStatus(id);
            if (!success)
                return NotFound();

            return Ok(lastUpdatedOn);
        }

        public IActionResult AllowedName(BrandFormViewModel model)
        {
            var isAllowed = _brandService.AllowedName(model);
            return Json(isAllowed);
        }

        public IActionResult AllowedCode(BrandFormViewModel model)
        {          
            if (!_brandService.AllowedCode(model)) // Not Allowed Code
                return Json("Brand code is already taken.");

            if (_brandService.IsCodeUsedInSKU(model)) // Used Sku
                return Json($"This code is already used in existing SKUs and cannot be changed.");

            return Json(true); // Allowed Code
        }


    }
}
