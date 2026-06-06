using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class CustomerProfileRepository : GenericRepository<CustomerProfile>, ICustomerProfileRepository
    {
        public CustomerProfileRepository(AppDbContext context) : base(context) { }

        public async Task<CustomerProfile?> GetByIdWithDetailsAsync(Guid id)
            => await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.CurrentTier)
                .Include(cp => cp.PointLogs.OrderByDescending(pl => pl.CreatedAt).Take(20))
                .FirstOrDefaultAsync(cp => cp.Id == id);

        public async Task<IEnumerable<CustomerProfile>> GetAllWithDetailsAsync()
            => await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.CurrentTier)
                .OrderBy(cp => cp.CurrentTier.PriorityLevel)
                .ToListAsync();

        public async Task<IEnumerable<CustomerProfile>> GetByTierIdAsync(Guid tierId)
            => await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.CurrentTier)
                .Where(cp => cp.CurrentTierId == tierId)
                .ToListAsync();
    }
}
