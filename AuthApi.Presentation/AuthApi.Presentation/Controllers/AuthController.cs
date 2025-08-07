using AuthApi.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _cache;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IDistributedCache cache,
            RoleManager<ApplicationRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cache = cache;
            _roleManager = roleManager;
        }

        [HttpPost("~/connect/token")]
        [HttpGet("~/connect/token")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ConnectToken()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username!);
                if (user == null || !await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = Errors.InvalidGrant,
                        ErrorDescription = "User does not exist or cannot sign in."
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = Errors.InvalidGrant,
                        ErrorDescription = result.IsLockedOut ? "User is locked out." : "Invalid username or password."
                    });
                }

                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                principal.SetScopes(request.GetScopes().ToList());

                var identity = (ClaimsIdentity)principal.Identity!;
                identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
                identity.AddClaim(new Claim(Claims.Audience, "resource"));
                identity.SetDestinations(GetDestinations);

                var properties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                return SignIn(principal, properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.UnsupportedGrantType,
                    ErrorDescription = "The specified grant type is not supported."
                });
            }
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            return claim.Type switch
            {
                Claims.Name or Claims.Subject => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken },
            };
        }

        [HttpGet("~/connect/userinfo")]
        [Produces("application/json")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserinfoAsync()
        {
            var subjectId = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
            if (string.IsNullOrEmpty(subjectId))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Token doesn't contain a subject claim."
                });
                return Challenge(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var cacheKey = $"UserInfo-{subjectId}";
            var cachedBytes = await _cache.GetAsync(cacheKey);
            Dictionary<string, object?> claims;

            if (cachedBytes != null)
            {
                claims = JsonSerializer.Deserialize<Dictionary<string, object?>>(Encoding.UTF8.GetString(cachedBytes))!;
            }
            else
            {
                var user = await _userManager.FindByIdAsync(subjectId);
                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    });
                    return Challenge(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                claims = new(StringComparer.Ordinal)
                {
                    [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user),
                    [OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user),
                    [OpenIddictConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user),
                    ["username"] = user.UserName
                };

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    claims[OpenIddictConstants.Claims.Role] = roles.First();
                }

                var serialized = JsonSerializer.Serialize(claims);
                await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(serialized),
                    new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }

            return Ok(claims);
        }

    }
}
