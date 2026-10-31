using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using WezProjectifierBLL;
using WezProjectifierBLL.DTOs.Login;
using WezProjectifierBLL.Models;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthPolicy")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly clsAuthService authService;
        public AuthController(clsAuthService authService)
        {
            this.authService = authService;
        }
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody]LoginRequestDTO dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await authService.LoginAsync(dto.Username,dto.Password,ip);

            return result.Result switch 
            {
                enOperationResult.Success => Ok(result),
                enOperationResult.InvalidData => BadRequest("Username/Password is wrong"),
                enOperationResult.InvalidCredentials => BadRequest("Username/Password is wrong"),
                _ => Problem(
                    statusCode:500,
                    title:"Internal server error",
                    detail:"Hold on until we fix the issue"
                )
            };
        }

        [HttpGet("refresh")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await authService.RefreshTokenAsync(refreshToken, ip);

            return result.Result switch
            { 
                enOperationResult.Success => Ok(result),
                enOperationResult.NotFound => NotFound("Enter a valid refresh token"),
                enOperationResult.Expired => BadRequest("Login again to access the system"),
                _ => Problem(
                    statusCode:500,
                    title:"Internal server error",
                    detail:"Hold on until we fix the issue"
                )
            };
        }

        [HttpGet("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            var result = await authService.LogoutAsync(userId);
            if (result)
            {
                return Ok("Logged out succesfuly");
            }
            
            return BadRequest("User has been deleted from the system propaply");
         
        }
    }
}
