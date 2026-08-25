using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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



        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Otp))
            {
                return BadRequest(new ApiResponse<object>(false, "Email and OTP are required.", null));
            }

            await _authService.VerifyOtpAsync(req);
            return Ok(new ApiResponse<object>(true, "Email verified successfully. You can now log in.", null));
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        public async Task<ActionResult> ResendOtp([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiResponse<object>(false, "Email is required.", null));
            }

            await _authService.ResendOtpAsync(email);
            return Ok(new ApiResponse<object>(true, "OTP verification code resent successfully.", null));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new ApiResponse<object>(false, "Email and password are required.", null));
            }

            var result = await _authService.LoginAsync(req);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken ?? string.Empty, cookieOptions);

            return Ok(new ApiResponse<object>(true, "Sign-in successful.", new { AccessToken = result.AccessToken, RequiresPasswordChange = result.RequiresPasswordChange }));
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.IdToken))
            {
                return BadRequest(new ApiResponse<object>(false, "Google IdToken is required.", null));
            }

            var result = await _authService.LoginWithGoogleAsync(req);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken ?? string.Empty, cookieOptions);

            return Ok(new ApiResponse<object>(true, "Google sign-in successful.", new { AccessToken = result.AccessToken, RequiresPasswordChange = result.RequiresPasswordChange }));
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult> SearchUsers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 3)
            {
                return BadRequest(new ApiResponse<object>(false, "Search query must be at least 3 characters long.", null));
            }
            return Ok(new ApiResponse<List<UserSearchResponse>>(true, "Success", await _authService.SearchUsersAsync(q)));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new ApiResponse<object>(false, "Refresh token not found.", null));
            }

            var result = await _authService.RefreshAsync(refreshToken);
            return Ok(new ApiResponse<object>(true, "Token refreshed successfully.", new { AccessToken = result.AccessToken }));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None
            });

            return Ok(new ApiResponse<object>(true, "Signed out successfully.", null));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            if (req is null)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid data.", null));
            }


            await _authService.ChangePasswordAsync(CurrentUserId, req);
            return Ok(new ApiResponse<object>(true, "Password changed successfully.", null));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new ApiResponse<object>(false, "Email is required.", null));
            }

            await _authService.ForgotPasswordAsync(req);
            return Ok(new ApiResponse<object>(true, "If the email is registered, an OTP will be sent.", null));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Otp) || string.IsNullOrWhiteSpace(req.NewPassword))
            {
                return BadRequest(new ApiResponse<object>(false, "All fields are required.", null));
            }

            await _authService.ResetPasswordAsync(req);
            return Ok(new ApiResponse<object>(true, "Password has been reset successfully.", null));
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult Health()
        {
            return Ok(new ApiResponse<string>(true, "v2 API is healthy.", "OK"));
        }
    }
}
