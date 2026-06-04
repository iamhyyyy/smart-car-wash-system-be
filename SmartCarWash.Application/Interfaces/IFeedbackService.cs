using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackDto>> GetAllAsync();
        Task<FeedbackDto?> GetByIdAsync(Guid id);
        Task<List<FeedbackDto>> GetByCustomerIdAsync(Guid customerId);
        Task<int> CountByCustomerIdAsync(Guid customerId);
        Task<FeedbackDto> AddFeedbackAsync(CreateFeedbackDto dto);
        Task<bool> Update(Guid id, UpdateFeedbackDto dto);
    }
}