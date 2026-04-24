
namespace IronFuel.Domain.Consts
{
    public class AppRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string ProductManager = "ProductManager";
        public const string OrderManager = "OrderManager";

        /// <summary>Comma-separated for <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute.Roles"/>.</summary>
        public const string AdminOrProductManager = Admin + "," + ProductManager;
    }
}
