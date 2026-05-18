using IronFuel.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace IronFuel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [EnableRateLimiting("fixed")]
    [Authorize(Roles = AppRoles.Admin)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetCategories();

            return View(categories);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var categoryModel = _categoryService.Create(model);

            return PartialView("_CategoryRow", categoryModel);
        }

        [HttpGet, AjaxOnly]
        public IActionResult Edit(int id)
        {
            var model = _categoryService.BuildEditModel(id);

            if(model is null)
                return NotFound();

            return PartialView("_Form", model);
        }

        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var categoryModel = _categoryService.Update(model);

            return PartialView("_CategoryRow", categoryModel);
        }


        [HttpPost]
        public IActionResult ToggleStatus (int id)
        {
            var (success, lastUpdateOn) = _categoryService.toggleStatus(id);

            if (!success)
                return NotFound();

            return Ok(lastUpdateOn);
        }



        public IActionResult AllowedItem(CategoryFormViewModel model)
        {
            var isAllowed = _categoryService.AllowedItem(model);

            return Json(isAllowed);
        }


    }
}
