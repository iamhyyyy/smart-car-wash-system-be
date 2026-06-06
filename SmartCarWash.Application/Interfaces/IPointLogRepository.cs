using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Domain.Interfaces
{
    public interface IPointLogRepository : IGenericRepository<PointLog>
    {
        /// <summary>Lấy lịch sử điểm theo customerId, sắp xếp mới nhất</summary>
        Task<List<PointLog>> GetByCustomerIdAsync(Guid customerId);

        /// <summary>Lấy các điểm sắp hết hạn (expiry trong N ngày tới)</summary>
        Task<List<PointLog>> GetExpiringPointsAsync(int withinDays);

        /// <summary>Lấy các điểm đã hết hạn (ExpiresAt < now và chưa bị expire)</summary>
        Task<List<PointLog>> GetExpiredPointsAsync();

        /// <summary>Tổng điểm kiếm được của customer</summary>
        Task<int> GetTotalEarnedAsync(Guid customerId);
    }
}
