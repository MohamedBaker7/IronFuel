using IronFuel.Application.Common.Interfaces;
using IronFuel.Web.Core.DTO;
using Microsoft.IdentityModel.SecurityTokenService;
using OpenQA.Selenium;

namespace IronFuel.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IApplicationDbContext _context;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;
        private static readonly TimeSpan CartTTL = TimeSpan.FromDays(30);

        private static string CacheKey(Guid cartToken) => $"cart:{cartToken}";

        public CartService(IApplicationDbContext context, CacheService cacheService, IMapper mapper)
        {
            _context = context;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<CartViewModel> GetCartAsync(Guid cartToken, string? userId = null)
        {
            Cart? cart = null;

            var cacheKey = $"cart:{cartToken}";


            try
            {
                var cached = await _cacheService.GetAsync<CartViewModel>(cacheKey);
                if (cached is not null)
                    return cached;
            }
            catch
            {
                throw new Exception("Error retrieving cart from cache.");
            }

            if (!string.IsNullOrEmpty(userId))
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.Status == CartStatus.Active &&
                        c.ExpiresAt > DateTime.UtcNow);

                if (cart is not null)
                {
                    await MergeGuestCartAsync(cart, cartToken);

                    return _mapper.Map<CartViewModel>(cart);
                }
            }

            cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Flavour)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c =>
                    c.CartToken == cartToken &&
                    c.Status == CartStatus.Active &&
                    c.ExpiresAt > DateTime.UtcNow);

            if (cart is null)
            {
                cart = await CreateCartInternalAsync(cartToken, userId);
            }

            var viewModel = _mapper.Map<CartViewModel>(cart);

            try
            {
                await _cacheService.SetAsync(cacheKey, viewModel, CartTTL);
            }
            catch
            {

            }

            return viewModel;
        }

        public async Task<int> GetCartCountAsync(Guid cartToken, string? userId = null)
        {
            var cacheKey = $"cart-count:{cartToken}";

            var cached = await _cacheService.GetAsync<int>(cacheKey);

            if (cached > 0)
                return cached;

            var count = await _context.Carts
                .Where(c =>
                    (!string.IsNullOrEmpty(userId) ? c.UserId == userId : c.CartToken == cartToken)
                    && c.Status == CartStatus.Active
                    && c.ExpiresAt > DateTime.UtcNow)
                .SelectMany(c => c.Items)
                .SumAsync(i => i.Quantity);

            await _cacheService.SetAsync(cacheKey, count, TimeSpan.FromMinutes(5));
            return count;
        }

        public async Task<CartViewModel> CreateCartAsync(CreateCartDto dto)
        {
            if (!string.IsNullOrEmpty(dto.UserId))
            {
                var existingCart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => new { pv.Flavour, pv.Product })
                    .FirstOrDefault(c =>
                    c.UserId == dto.UserId
                    && c.Status == CartStatus.Active
                    && c.ExpiresAt > DateTime.UtcNow);


                if (existingCart != null)
                    return _mapper.Map<CartViewModel>(existingCart);
            }

            var cart = new Cart()
            {
                CartToken = Guid.NewGuid(),
                UserId = dto.UserId,
                Status = CartStatus.Active,
                ExpiresAt = DateTime.UtcNow.Add(CartTTL)
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            InvalidateCache(cart.CartToken);

            var cartViewModel = _mapper.Map<CartViewModel>(cart);

            await _cacheService.SetAsync(CacheKey(cart.CartToken), cartViewModel, CartTTL);

            return cartViewModel;
        }

        public async Task<CartViewModel> AddItemAsync(Guid cartToken, string sku, int qty)
        {
            var variant = await _context.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.SKU == sku.ToUpper());

            if (variant is null)
                throw new NotFoundException($"SKU '{sku}' not found.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartToken == cartToken);

            if (cart is null || cart.Status != CartStatus.Active || cart.ExpiresAt < DateTime.UtcNow)
                throw new BadRequestException("Cart is not active.");

            if (variant.Stock < qty)
                throw new BadRequestException($"Only {variant.Stock} units available for SKU '{sku}'.");

            const int MaxQtyPerItem = 10;
            var existing = cart.Items.FirstOrDefault(i => i.SKU == sku.ToUpper());
            var totalQty = qty + (existing?.Quantity ?? 0);

            if (totalQty > MaxQtyPerItem)
                throw new BadRequestException($"Maximum {MaxQtyPerItem} units per item.");

            if (existing is not null)
            {
                // SKU already in cart — increment
                existing.Quantity += qty;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,
                    SKU = variant.SKU,
                    UnitPrice = variant.Price,
                    Quantity = qty
                });
            }

            // TODO: WE will remove this and add it again after succeeded checkout
            variant.Stock -= qty;
            _context.ProductVariants.Update(variant);

            await _context.SaveChangesAsync();

            InvalidateCache(cart.CartToken);

            var fullCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Flavour)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.CartToken == cartToken);

            var cartModel = _mapper.Map<CartViewModel>(fullCart);

            await _cacheService.SetAsync(CacheKey(cartToken), cartModel, CartTTL);

            return cartModel;
        }

        public bool UpdateCartItem(string sku, int qty)
        {
            if (qty < 1) return false;

            var cartItem = _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefault(c => c.SKU == sku);

            if (cartItem == null) return false;

            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.SKU == sku.ToUpper());

            if (variant is not null)
            {
                var diff = qty - cartItem.Quantity;

                if (diff > 0 && variant.Stock < diff)
                    return false; 

                variant.Stock -= diff;
                _context.ProductVariants.Update(variant);
            }

            cartItem.Quantity = qty;
            _context.SaveChanges();

            InvalidateCache(cartItem.Cart.CartToken);
            return true;
        }

        public bool RemoveCartItem(string sku)
        {
            var cartItem = _context.CartItems
                .Include(c => c.Cart)
                .FirstOrDefault(c => c.SKU == sku);

            if (cartItem == null) return false;

            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.SKU == sku.ToUpper());

            if (variant is not null)
            {
                variant.Stock += cartItem.Quantity;
                _context.ProductVariants.Update(variant);
            }

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            InvalidateCache(cartItem.Cart.CartToken);
            return true;
        }


        public decimal GetItemSubtotal(string sku)
        {
            return _context.CartItems
                .Include(c => c.ProductVariant)
                .Where(c => c.SKU == sku)
                .Select(c => c.ProductVariant.Price * c.Quantity)
                .FirstOrDefault();
        }

        public decimal GetCartTotal()
        {
            return _context.CartItems
                .Include(c => c.ProductVariant)
                .Sum(c => c.ProductVariant.Price * c.Quantity);
        }

        public int GetCartCount()
        {
            return _context.CartItems.Count();
        }

        private static CartDto MapToDto(Cart cart) => new()
        {
            Id = cart.Id,
            CartToken = cart.CartToken,
            UserId = cart.UserId,
            Status = cart.Status,
            ExpiresAt = cart.ExpiresAt,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant.Product.Name,
                FlavourName = i.ProductVariant.Flavour.Name,
                WeightG = i.ProductVariant.WeightG,
                SKU = i.SKU,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
            }).ToList()
        };

        private async Task MergeGuestCartAsync(Cart userCart, Guid guestToken)
        {
            // Find the guest cart
            var guestCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.CartToken == guestToken &&
                    c.Status == CartStatus.Active);

            if (guestCart is null || !guestCart.Items.Any())
                return;

            foreach (var guestItem in guestCart.Items)
            {
                var existing = userCart.Items
                    .FirstOrDefault(i => i.SKU == guestItem.SKU);

                if (existing is not null)
                {
                    existing.Quantity += guestItem.Quantity;
                }
                else
                {
                    userCart.Items.Add(new CartItem
                    {
                        CartId = userCart.Id,
                        ProductVariantId = guestItem.ProductVariantId,
                        SKU = guestItem.SKU,
                        UnitPrice = guestItem.UnitPrice,
                        Quantity = guestItem.Quantity
                    });
                }
            }

            guestCart.Status = CartStatus.Expired;

            await _context.SaveChangesAsync();
            InvalidateCache(guestToken);

            var guestCartViewModel = _mapper.Map<CartViewModel>(guestCart);

            await _cacheService.SetAsync(CacheKey(userCart.CartToken), guestCartViewModel, TimeSpan.FromDays(30));


        }

        private async Task<Cart> CreateCartInternalAsync(Guid cartToken, string? userId)
        {
            var cart = new Cart
            {
                CartToken = cartToken,
                UserId = userId,
                Status = CartStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            await _cacheService.SetAsync(CacheKey(cartToken), _mapper.Map<CartViewModel>(cart), TimeSpan.FromDays(30));

            return cart;
        }

        private async void InvalidateCache(Guid cartToken)
        {
            await _cacheService.RemoveAsync($"cart:{cartToken}");
            await _cacheService.RemoveAsync($"cart-count:{cartToken}");
        }
    }
}
