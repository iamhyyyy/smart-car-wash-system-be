using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface ICustomerProfileRepository : IGenericRepository<CustomerProfile>
    {
        Task<CustomerProfile?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<CustomerProfile>> GetAllWithDetailsAsync();
        Task<IEnumerable<CustomerProfile>> GetByTierIdAsync(Guid tierId);
    }
}
