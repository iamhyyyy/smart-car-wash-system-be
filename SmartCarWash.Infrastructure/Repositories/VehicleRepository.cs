using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly AppDbContext _context;
        public VehicleRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Vehicles.Where(p => p.CustomerId == customerId).ToListAsync();

        }
    }
}