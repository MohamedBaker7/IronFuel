using IronFuel.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace IronFuel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [EnableRateLimiting("fixed")]
    [Authorize(Roles = AppRoles.Admin)]
    public class FlavorsController : Controller
    {
        private readonly IFlavorService _flavorService;

        public FlavorsController(IFlavorService flavorService)
        {
            _flavorService = flavorService;
        }

        public async Task<IActionResult> Index()
        {
            var flavors = await _flavorService.GetFlavors();
            return View(flavors);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public IActionResult Create(FlavorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var flavorModel = _flavorService.Create(model);
            return PartialView("_FlavorRow", flavorModel);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Edit(int id)
        {
            var model = _flavorService.BuildEditModel(id);
            if (model is null)
                return NotFound();

            return PartialView("_Form", model);
        }

        [HttpPost]
        public IActionResult Edit(FlavorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var flavorModel = _flavorService.Update(model);
            if (flavorModel is null)
                return NotFound();

            return PartialView("_FlavorRow", flavorModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var (success, lastUpdatedOn) = _flavorService.ToggleStatus(id);
            if (!success)
                return NotFound();

            return Ok(lastUpdatedOn);
        }

        public IActionResult AllowedItem(FlavorFormViewModel model)
        {
            var isAllowed = _flavorService.AllowedItem(model);
            return Json(isAllowed);
        }
    }
}
