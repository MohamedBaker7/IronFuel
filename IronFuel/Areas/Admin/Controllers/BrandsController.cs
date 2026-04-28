using IronFuel.Web.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace IronFuel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [EnableRateLimiting("fixed")]
    public class BrandsController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        public IActionResult Index()
        {
            var brands = _brandService.GetBrands();
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

        public IActionResult AllowedItem(BrandFormViewModel model)
        {
            var isAllowed = _brandService.AllowedItem(model);
            return Json(isAllowed);
        }
    }
}
