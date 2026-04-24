namespace IronFuel.Web.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public static string GetUsername(this ClaimsPrincipal user) =>
             user.FindFirstValue(ClaimTypes.Name)!;
    }
}
