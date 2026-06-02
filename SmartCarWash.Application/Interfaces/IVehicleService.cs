using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllAsync();
        Task<VehicleDto?> GetByIdAsync(Guid id);
        Task<List<VehicleDto>> GetByCustomerIdAsync(Guid customerId);
        Task<int> CountByCustomerIdAsync(Guid customerId);
        Task<VehicleDto> AddVehicleAsync(CreateVehicleDto dto);
        Task<bool> Update(Guid id, UpdateVehicleDto dto);
    }
}