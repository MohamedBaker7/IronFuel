using IronFuel.Web.Core.DTO;

namespace IronFuel.Web.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(Guid cartToken, string? userId = null);
        Task<int> GetCartCountAsync(Guid cartToken, string? userId = null);     
        Task<CartViewModel> CreateCartAsync(CreateCartDto dto);
        Task<CartViewModel> AddItemAsync(Guid cartToken, string sku, int qty);
        bool UpdateCartItem(string sku, int qty);
        bool RemoveCartItem(string sku);
        decimal GetItemSubtotal(string sku);
        decimal GetCartTotal();
        int GetCartCount();

    }
}
