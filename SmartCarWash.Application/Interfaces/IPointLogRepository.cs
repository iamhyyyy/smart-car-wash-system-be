using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Domain.Interfaces
{
    public interface IPointLogRepository : IGenericRepository<PointLog>
    {
        Task<List<PointLog>> GetByCustomerIdAsync(Guid customerId);
        Task<List<PointLog>> GetExpiringPointsAsync(int withinDays);
        Task<List<PointLog>> GetExpiredPointsAsync();
        Task<int> GetTotalEarnedAsync(Guid customerId);
    }
}
