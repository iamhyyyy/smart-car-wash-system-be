using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class PointLogRepository : GenericRepository<PointLog>, IPointLogRepository
    {
        public PointLogRepository(AppDbContext context) : base(context) { }

        public async Task<List<PointLog>> GetByCustomerIdAsync(Guid customerId)
            => await _context.PointLogs
                .Where(pl => pl.CustomerId == customerId)
                .OrderByDescending(pl => pl.CreatedAt)
                .ToListAsync();

        public async Task<List<PointLog>> GetExpiringPointsAsync(int withinDays)
        {
            var cutoff = DateTime.UtcNow.AddDays(withinDays);
            return await _context.PointLogs
                .Include(pl => pl.Customer)
                    .ThenInclude(cp => cp.User)
                .Where(pl =>
                    pl.ExpiresAt.HasValue &&
                    pl.ExpiresAt.Value <= cutoff &&
                    pl.ExpiresAt.Value > DateTime.UtcNow &&
                    pl.TransactionType == PointTransactionType.Earn)
                .OrderBy(pl => pl.ExpiresAt)
                .ToListAsync();
        }

        public async Task<List<PointLog>> GetExpiredPointsAsync()
            => await _context.PointLogs
                .Where(pl =>
                    pl.ExpiresAt.HasValue &&
                    pl.ExpiresAt.Value < DateTime.UtcNow &&
                    pl.TransactionType == PointTransactionType.Earn)
                .ToListAsync();

        public async Task<int> GetTotalEarnedAsync(Guid customerId)
            => await _context.PointLogs
                .Where(pl => pl.CustomerId == customerId &&
                             (pl.TransactionType == PointTransactionType.Earn ||
                              pl.TransactionType == PointTransactionType.Bonus))
                .SumAsync(pl => pl.PointsChanged);
    }
}
