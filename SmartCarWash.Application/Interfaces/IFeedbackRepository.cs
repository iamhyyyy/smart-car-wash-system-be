using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface IFeedbackRepository : IGenericRepository<Feedback>
    {
        Task<List<Feedback>> GetByCustomerIdAsync(Guid customerId);

        //Task<Feedback> GetByIdAsync(Guid id);
    }
}