using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto> CreateUserAsync(CreateUserDto dto, string role = "customer");
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto);
        Task<bool> LockUserAsync(Guid id);
        Task<bool> UnlockUserAsync(Guid id);
        Task<bool> ChangePasswordAsync(Guid id, string currentPassword, string newPassword);
        Task<IEnumerable<string>> GetRolesAsync(Guid id);
        Task<bool> AssignRoleAsync(Guid id, string role);
    }
}
