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
        Task<int> RunMonthlyTierReviewAsync();
    }
}
