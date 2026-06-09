using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<List<Booking>> GetByCustomerIdAsync(Guid customerId);
        Task<int> CountPromoUsagesByCustomerAsync(Guid promoId, Guid customerId);
    }
}