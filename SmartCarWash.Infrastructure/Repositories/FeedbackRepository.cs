using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Feedback>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Feedbacks
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();

        }

        public override async Task<Feedback?> GetByIdAsync(Guid id)
        {
            return await _context.Feedbacks
                .Include(p => p.Customer)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}