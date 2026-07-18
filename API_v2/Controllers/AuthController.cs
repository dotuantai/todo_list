using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public ActionResult Register([FromBody] RegisterRequest req)
        {
            if (req is null)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid data.", null));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiResponse<object>(false, string.Join(" | ", errors), null));
            }

            _authService.Register(req);
            return Ok(new ApiResponse<object>(true, "Registration successful.", null));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult Login([FromBody] LoginRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new ApiResponse<object>(false, "Email and password are required.", null));
            }

            var result = _authService.Login(req);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken ?? string.Empty, cookieOptions);

            return Ok(new ApiResponse<object>(true, "Sign-in successful.", new { AccessToken = result.AccessToken }));
        }

        [HttpGet("search")]
        [Authorize]
        public ActionResult SearchUsers([FromQuery] string q)
        {
            return Ok(new ApiResponse<List<UserSearchResponse>>(true, "Success", _authService.SearchUsers(q)));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public ActionResult Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new ApiResponse<object>(false, "Refresh token not found.", null));
            }

            var result = _authService.Refresh(refreshToken);
            return Ok(new ApiResponse<object>(true, "Token refreshed successfully.", new { AccessToken = result.AccessToken }));
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                _authService.Logout(refreshToken);
            }

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Path = "/",
                SameSite = SameSiteMode.Lax
            });

            return Ok(new ApiResponse<object>(true, "Signed out successfully.", null));
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult Health()
        {
            return Ok(new ApiResponse<string>(true, "v2 API is healthy.", "OK"));
        }
    }
}
