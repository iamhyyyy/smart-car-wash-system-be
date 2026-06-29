using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IPointLogService
    {
        Task<IEnumerable<PointLogDto>> GetAllAsync();
        Task<PointLogDto?> GetByIdAsync(Guid id);
        Task<List<PointLogDto>> GetByCustomerIdAsync(Guid customerId);
        Task<PointLogDto> CreateAsync(CreatePointLogDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<int> ProcessExpiredPointsAsync();
        Task<List<PointLogDto>> GetExpiringPointsAsync(int withinDays = 30);
    }
}
