using Microsoft.EntityFrameworkCore;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Repositories
{
    public class WashServiceRepository : GenericRepository<WashService>, IWashServiceRepository
    {
        public WashServiceRepository(AppDbContext context) : base(context)
        {
        }
    }
}