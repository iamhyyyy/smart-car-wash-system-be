using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class TierRepository : GenericRepository<Tier>, ITierRepository
    {
        public TierRepository(AppDbContext context) : base(context) { }

        public async Task<Tier?> GetByNameAsync(string name)
            => await _context.Tiers.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

        public async Task<IEnumerable<Tier>> GetActiveAsync()
            => await _context.Tiers
                .Where(t => t.IsActive)
                .OrderBy(t => t.PriorityLevel)
                .ToListAsync();
    }
}
