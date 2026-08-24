using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs;
using StockFlow.Interfaces;

namespace StockFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
      private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _authService.RegisterAsync(request);
            return Ok(response);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return Ok(new {message = "Token revoked successfully"});
        }
    }
}
