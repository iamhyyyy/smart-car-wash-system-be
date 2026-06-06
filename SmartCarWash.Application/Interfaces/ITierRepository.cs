using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface ITierRepository : IGenericRepository<Tier>
    {
        Task<Tier?> GetByNameAsync(string name);
        Task<IEnumerable<Tier>> GetActiveAsync();
    }
}
