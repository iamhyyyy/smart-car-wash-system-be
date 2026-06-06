using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs.Auth;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        if (result.IsSuccess)
            return Ok(result);

        return Unauthorized(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Thiếu thông tin xác nhận." });

        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
