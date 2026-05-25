using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Domain.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        // Thêm hàm riêng nếu cần, ví dụ: Task<Vehicle?> GetByLicensePlateAsync(string plate);
    }
}