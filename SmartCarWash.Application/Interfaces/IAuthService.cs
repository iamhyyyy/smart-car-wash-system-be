using SmartCarWash.Application.DTOs.Auth;

namespace SmartCarWash.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
}
