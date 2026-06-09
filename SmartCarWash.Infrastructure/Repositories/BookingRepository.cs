using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Booking>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Bookings.Where(p => p.CustomerId == customerId).ToListAsync();
        }

        public async Task<int> CountPromoUsagesByCustomerAsync(Guid promoId, Guid customerId)
        {
            return await _context.Bookings.CountAsync(b =>
                b.PromoId == promoId &&
                b.CustomerId == customerId &&
                b.Status != BookingStatus.Cancelled);
        }

        public override async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(p => p.Customer)
                    .ThenInclude(p => p.User)
                .Include(p => p.Customer)
                    .ThenInclude(p => p.CurrentTier)
                .Include(p => p.Vehicle)
                .Include(p => p.Service)
                .Include(p => p.Promo)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}