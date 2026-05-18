using IronFuel.Web.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace IronFuel.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _roleManager = roleManager;
            _logger = logger;
        }


        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            {
                return new AuthModel { Message = "Email is already registered!" };
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthModel
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            await _userManager.AddToRoleAsync(user, AppRoles.Customer);

            var jwt = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("New user registered: {Email}", user.Email);

            return new AuthModel
            {
                IsAuthenticated = true,
                Email = user.Email,
                Roles = roles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpireOn = jwt.ValidTo,
            };

        }
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || user.IsDeleted)
                return new AuthModel { Message = "Email or Password is incorrect!" };

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Locked-out login attempt: {Email}", model.Email);
                return new AuthModel { Message = "Account locked. Try again later." };
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Email or Password is incorrect!" };

            await _userManager.ResetAccessFailedCountAsync(user);

            var jwt = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);


            var authModel = new AuthModel
            {
                IsAuthenticated = true,
                Email = user.Email,
                Roles = roles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpireOn = jwt.ValidTo
            };

            var activeToken = user.RefreshTokens!.FirstOrDefault(r => r.IsActive);

            if (activeToken is not null)
            {

                authModel.RefreshToken = activeToken!.Token;
                authModel.RefreshTokenExpiration = activeToken.ExpiresOn;

            }
            else
            {
                var refreshToken = GenerateRefreshToken();

                user.RefreshTokens!.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
            }
            _logger.LogInformation("User logged in: {Email}", user.Email);
            return authModel;
        }

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
                return "Invalid user ID or role name!";

            if (await _userManager.IsInRoleAsync(user, model.RoleName))
                return "User is already assigned to this role!";

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            return result.Succeeded ? string.Empty : "Something went wrong!";

        }

        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens!.Any(r => r.Token == token));

            if (user is null)
                return new AuthModel { Message = "Invalid token" };

            var refreshToken = user.RefreshTokens!.Single(r => r.Token == token);

            if (!refreshToken.IsActive)
                return new AuthModel { Message = "Inactive token" };

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens!.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwt = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthModel
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Email = user.Email,
                Roles = roles.ToList(),
                ExpireOn = jwt.ValidTo,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens!.Any(r => r.Token == token));

            if (user is null)
                return false;

            var refreshToken = user.RefreshTokens!.Single(r => r.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Refresh token revoked for user: {Email}", user.Email);
            return true;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: credentials);

            return jwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var bytes = new byte[32];

            RandomNumberGenerator.Fill(bytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(bytes),
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };
        }


    }
}
