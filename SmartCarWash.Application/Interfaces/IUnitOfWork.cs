namespace SmartCarWash.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> CompleteAsync(); // Thay thế cho SaveChangesAsync
        IVehicleRepository VehicleRepository { get; }
        IFeedbackRepository FeedbackRepository { get; }
        IPromotionRepository PromotionRepository { get; }
    }
}