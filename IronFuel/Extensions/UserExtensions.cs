namespace IronFuel.Web.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        public static string GetUsername(this ClaimsPrincipal user) =>
             user.FindFirst(ClaimTypes.Name)!.Value;
    }
}
