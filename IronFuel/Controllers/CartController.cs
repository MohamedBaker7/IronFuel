using IronFuel.Web.Core.DTO;
using IronFuel.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.SecurityTokenService;
using OpenQA.Selenium;

namespace IronFuel.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var token = GetCartToken();

            var cartModel = await _cartService.GetCartAsync(token);

            return View(cartModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string sku, int qty = 1)
        {
            try
            {
                var userId = User.GetUserId();
                var cartToken = Request.Cookies["CartToken"];

                CartViewModel cart;

                if (cartToken == null)
                {
                    cart = await _cartService.CreateCartAsync(new CreateCartDto { UserId = userId });
                    cartToken = cart.CartToken.ToString();

                    Response.Cookies.Append("CartToken", cartToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
                }

                var updatedCart = await _cartService.AddItemAsync(Guid.Parse(cartToken), sku, qty);

                var addedItem = updatedCart.Items
                    
                    .FirstOrDefault(i => i.SKU == sku.ToUpper());

                return Json(new
                {
                    success = true,
                    cart_count = updatedCart.Items.Sum(i => i.Quantity),
                    cart_total = updatedCart.TotalAmount,
                    item = new
                    {
                        name = addedItem?.ProductName,
                        image = addedItem?.ProductImage,
                        price = addedItem?.UnitPrice,
                        totalPrice = addedItem?.TotalPrice,
                        flavour = addedItem?.FlavourName,
                        size = addedItem?.WeightG,
                        servings = addedItem?.Servings
                    }
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(string sku , int qty)
        {
            if(qty < 1)
                return BadRequest(new { success = false, message = "Quantity must be at least 1" });

            try
            {
                var result = _cartService.UpdateCartItem(sku, qty);

                if (!result)
                    return NotFound(new { success = false, message = "Item not found" });

                var subtotal = _cartService.GetItemSubtotal(sku);
                var cartTotal = _cartService.GetCartTotal();

                return Json(new
                {
                    success = true,
                    message = "Cart updated successfully",
                    sub_total = subtotal,
                    cart_total = cartTotal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string sku)
        {
            try
            {
                var result = _cartService.RemoveCartItem(sku);

                if (!result)
                    return NotFound(new { success = false, message = "Item not found" });

                var cartTotal = _cartService.GetCartTotal();
                var cartCount = _cartService.GetCartCount();

                return Json(new
                {
                    success = true,
                    message = "Item removed successfully",
                    cart_total = cartTotal,
                    cart_count = cartCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CartSummary()
        {
            var cartToken = GetCartToken();

            var cart = await _cartService.GetCartAsync(cartToken);

            return PartialView("_CartSummaryBody", cart);
        }

        private Guid GetCartToken()
        {
            // Try to get existing token from cookie
            if (Request.Cookies.TryGetValue("CartToken", out var raw)
                && Guid.TryParse(raw, out var token))
                return token;

            // Create new token and store in cookie
            var newToken = Guid.NewGuid();
            Response.Cookies.Append("CartToken", newToken.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return newToken;
        }
    }
}
