using IronFuel.Web.Services;
using Microsoft.AspNetCore.Authorization;

[Area("Admin")]
[Route("api/[area]/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(model);
        return result.IsAuthenticated ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TokenRequestModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.GetTokenAsync(model);
        if (!result.IsAuthenticated)
            return Unauthorized(result.Message);

        SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiration);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        // read from secure cookie (not body — safer)
        var token = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest("Refresh token is required.");

        var result = await _authService.RefreshTokenAsync(token);
        if (!result.IsAuthenticated)
            return Unauthorized(result.Message);

        SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiration);
        return Ok(result);
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        // accept token from body OR cookie
        var token = request.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required.");

        var result = await _authService.RevokeTokenAsync(token);
        return result ? Ok("Token revoked.") : BadRequest("Invalid token.");
    }

    [HttpPost("add-role")]
    [Authorize(Roles = "Admin")]   // only admins can assign roles
    public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AddRoleAsync(model);
        return string.IsNullOrEmpty(result) ? Ok(model) : BadRequest(result);
    }

    // ── Helpers ────────────────────────────────────────────────
    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expires
        });
    }
}

public record RevokeTokenRequest(string Token);
