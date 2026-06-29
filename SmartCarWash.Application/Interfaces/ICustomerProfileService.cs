using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface ICustomerProfileService
    {
        Task<IEnumerable<CustomerProfileDto>> GetAllAsync();
        Task<CustomerProfileDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<CustomerProfileDto>> GetByTierIdAsync(Guid tierId);
        Task<CustomerProfileDto> CreateAsync(CreateCustomerProfileDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateCustomerProfileDto dto);
        Task<bool> RedeemPointsAsync(Guid customerId, int pointsToRedeem, string note);
        Task<bool> AddPointsAsync(Guid customerId, int points, string note, Guid? bookingId = null);
    }
}
