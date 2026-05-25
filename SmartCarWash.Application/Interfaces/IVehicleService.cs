using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleDto> AddVehicleAsync(CreateVehicleDto dto);
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
    }
}