using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IronFuel.Web.Areas.Identity.Pages.Account
{
    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string UserEmail { get; private set; } = string.Empty;

        // Placeholder list until an Order entity/service is introduced.
        public List<string> OrderNumbers { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserEmail = user?.Email ?? User.Identity?.Name ?? "Current user";
        }
    }
}
