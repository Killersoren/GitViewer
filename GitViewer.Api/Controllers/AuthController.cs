using GitViewer.Api.Dto;
using GitViewer.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        public AuthController(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("Username already exists");
            }

            await _loggingService.LogAccountCreatedAsync(user);

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid username or password");
            }

            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken is null)
            {
                return Ok();
            }

            var user = await _authService.GetUserByRefreshTokenAsync(refreshToken);
            if (user != null)
            {
                await _authService.DeleteRefreshTokenAsync(user);
            }

            DeleteRefreshTokenCookie();
            return Ok("Logged out successfully");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken is null)
            {
                return Unauthorized("No refresh token");
            }

            var result = await _authService.RefreshTokensAsync(refreshToken);
            if (result is null)
            {
                return Unauthorized("Invalid refresh token");
            }

            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken });
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are an admin");
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        }
    }
}
