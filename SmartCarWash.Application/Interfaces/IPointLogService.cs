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

        /// <summary>
        /// Hết hạn điểm: quét tất cả PointLog đã Earn có ExpiresAt < now,
        /// trừ điểm vào AvailablePoints của customer và ghi log Expire
        /// </summary>
        Task<int> ProcessExpiredPointsAsync();

        /// <summary>
        /// Lấy các bản ghi điểm sắp hết hạn trong N ngày tới (để notify)
        /// </summary>
        Task<List<PointLogDto>> GetExpiringPointsAsync(int withinDays = 30);
    }
}
