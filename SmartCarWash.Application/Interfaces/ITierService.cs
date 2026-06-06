using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface ITierService
    {
        Task<IEnumerable<TierDto>> GetAllAsync();
        Task<IEnumerable<TierDto>> GetActiveAsync();
        Task<TierDto?> GetByIdAsync(Guid id);
        Task<TierDto> CreateAsync(CreateTierDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateTierDto dto);
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Chạy monthly review: tự động upgrade/downgrade tier cho tất cả customer
        /// dựa vào LifetimePoints so sánh với MinPointsRequired của các Tier
        /// </summary>
        Task<int> RunMonthlyTierReviewAsync();
    }
}
