using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllAsync();
        Task<PromotionDto?> GetByIdAsync(Guid id);
        Task<PromotionDto> AddPromotionAsync(CreatePromotionDto dto);
        Task<bool> Update(Guid id, UpdatePromotionDto dto);
        Task<bool> Delete(Guid id);
    }
}