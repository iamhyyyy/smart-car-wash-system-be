using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;
using System.Collections;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private Hashtable? _repositories;
        private IVehicleRepository _vehicleRepository = null!;
        private IFeedbackRepository _feedbackRepository = null!;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            _repositories ??= new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance =
                    Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }

        public IVehicleRepository VehicleRepository => _vehicleRepository ??= new VehicleRepository(_context);
        public IFeedbackRepository FeedbackRepository => _feedbackRepository ??= new FeedbackRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}