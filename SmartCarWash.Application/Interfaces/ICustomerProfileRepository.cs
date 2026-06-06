using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface ICustomerProfileRepository : IGenericRepository<CustomerProfile>
    {
        /// <summary>Lấy CustomerProfile kèm User, Tier, PointLogs</summary>
        Task<CustomerProfile?> GetByIdWithDetailsAsync(Guid id);

        /// <summary>Lấy tất cả CustomerProfile kèm User và Tier</summary>
        Task<IEnumerable<CustomerProfile>> GetAllWithDetailsAsync();

        /// <summary>Lấy CustomerProfile theo TierId</summary>
        Task<IEnumerable<CustomerProfile>> GetByTierIdAsync(Guid tierId);
    }
}
