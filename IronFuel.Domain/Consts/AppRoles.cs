
namespace IronFuel.Domain.Consts
{
    public class AppRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string ProductManager = "ProductManager";
        public const string OrderManager = "OrderManager";
        public const string AdminOrProductManager = Admin + "," + ProductManager;
    }
}
