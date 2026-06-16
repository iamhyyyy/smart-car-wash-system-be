using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCarWash.Application.DTOs.Auth;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Email đã tồn tại." };
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Mặc định gán role customer
        await _userManager.AddToRoleAsync(user, "customer");

        // Tạo CustomerProfile mặc định cho customer
        var defaultTier = (await _unitOfWork.TierRepository.GetActiveAsync())
                          .OrderBy(t => t.MinPointsRequired)
                          .FirstOrDefault();

        if (defaultTier != null)
        {
            var profile = new CustomerProfile
            {
                Id = user.Id,
                CurrentTierId = defaultTier.Id,
                AvailablePoints = 0,
                LifetimePoints = 0,
                TotalVisits = 0,
                TotalSpending = 0,
                LastTierReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CustomerProfileRepository.AddAsync(profile);
            await _unitOfWork.CompleteAsync();
        }

        // Sinh token xác nhận email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var clientUrl = _configuration["ClientUrl"];
        var confirmLink = $"{clientUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        // Gửi email xác nhận
        var emailSent = true;
        try
        {
            var subject = "[Smart Car Wash] Xác nhận tài khoản của bạn 🚗✨";
            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;border:1px solid #e0e0e0;border-radius:10px;">
                  <h2 style="color:#1a73e8;">Xin chào, {user.FirstName} {user.LastName}!</h2>
                  <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Smart Car Wash</strong> 🚿</p>
                  <p>Tài khoản của bạn đã được tạo thành công. Vui lòng nhấn vào nút bên dưới để xác nhận địa chỉ email và kích hoạt tài khoản.</p>
                  <br/>
                  <a href="{confirmLink}"
                     style="background:#1a73e8;color:white;padding:12px 28px;text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block;">
                    ✅ Xác nhận tài khoản
                  </a>
                  <br/><br/>
                  <p style="color:#888;font-size:13px;">⏳ Liên kết này sẽ hết hạn sau <strong>24 giờ</strong>. Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.</p>
                  <hr style="border:none;border-top:1px solid #eee;margin:20px 0;"/>
                  <p style="font-size:13px;color:#555;">Trân trọng,<br/><strong>Đội ngũ Smart Car Wash</strong> 🚗</p>
                </div>
                """;

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }
        catch (Exception)
        {
            emailSent = false;
        }

        var message = emailSent
            ? "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản."
            : "Đăng ký thành công! Tuy nhiên, hệ thống không thể gửi email xác nhận. Vui lòng liên hệ hỗ trợ.";

        return new AuthResponseDto { IsSuccess = true, Message = message };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Email hoặc mật khẩu không đúng." };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Tài khoản của bạn đã bị khóa." };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Tài khoản chưa được xác nhận email. Vui lòng kiểm tra hộp thư của bạn." };
        }

        // Tạo JWT Token
        var roles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Message = "Đăng nhập thành công!"
        };
    }

    public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Không tìm thấy tài khoản." };
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponseDto { IsSuccess = true, Message = "Email đã được xác nhận trước đó." };
        }

        //var decodedToken = HttpUtility.UrlDecode(token);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Xác nhận email thất bại. Liên kết không hợp lệ hoặc đã hết hạn."
            };
        }

        return new AuthResponseDto { IsSuccess = true, Message = "Xác nhận email thành công! Bạn có thể đăng nhập ngay bây giờ." };
    }
}
