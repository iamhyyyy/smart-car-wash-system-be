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
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Đổi điểm thưởng để nhận ưu đãi (Redemption)
        /// pointsToRedeem: số điểm muốn đổi
        /// note: mô tả ưu đãi đổi được (ví dụ: "Free wash", "10% discount")
        /// </summary>
        Task<bool> RedeemPointsAsync(Guid customerId, int pointsToRedeem, string note);

        /// <summary>
        /// Cộng điểm thủ công (Admin điều chỉnh hoặc điểm thưởng)
        /// </summary>
        Task<bool> AddPointsAsync(Guid customerId, int points, string note, Guid? bookingId = null);
    }
}
