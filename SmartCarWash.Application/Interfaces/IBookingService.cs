using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetAllAsync();
        Task<BookingDto?> GetByIdAsync(Guid id);
        Task<List<BookingDto>> GetByCustomerIdAsync(Guid customerId);
        Task<int> CountByCustomerIdAsync(Guid customerId);
        Task<BookingDto> AddBookingAsync(CreateBookingDto dto);
        Task<bool> Update(Guid id, UpdateBookingDto dto);
        Task<bool> Delete(Guid id);
    }
}