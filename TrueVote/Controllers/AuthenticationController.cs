using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;
using TrueVote.Models.DTOs;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;

        }
        [HttpPost("send-magic-link")]
        public async Task<IActionResult> SendMagicLink([FromBody] MagicLinkRequest request)
        {
            await _authenticationService.SendMagicLinkAsync(request);
            return Ok(new { message = "If registered, a login link has been sent." });
        }

        [HttpPost("verify-magic-link")]
        public async Task<IActionResult> VerifyMagicLink([FromBody] MagicLinkVerifyRequest request)
        {
            try
            {
                var result = await _authenticationService.VerifyMagicLinkAsync(request);
                return Ok(result);
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserLoginResponse>> UserLogin(UserLoginRequest loginRequest)
        {
            try
            {
                var result = await _authenticationService.Login(loginRequest);
                return Ok(result);
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<UserLoginResponse>> RefreshToken([FromBody] TokenRefreshRequest dto)
        {
            try
            {
                var result = await _authenticationService.RefreshLogin(dto.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] TokenRefreshRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest("Refresh token required.");

            var result = await _authenticationService.LogoutAsync(dto.RefreshToken);
            if (!result)
                return NotFound("Refresh token not found or already revoked.");

            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _authenticationService.GetCurrentUserAsync(username);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Role
            });
        }
        
    }
}