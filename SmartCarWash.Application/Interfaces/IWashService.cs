using SmartCarWash.Application.DTOs;

namespace SmartCarWash.Application.Interfaces
{
    public interface IWashService
    {
        Task<IEnumerable<WashServiceDto>> GetAllAsync();
        Task<WashServiceDto?> GetByIdAsync(Guid id);
        // Task<List<WashServiceDto>> GetByCustomerIdAsync(Guid customerId);
        // Task<int> CountByCustomerIdAsync(Guid customerId);
        Task<WashServiceDto> AddWashServiceAsync(CreateWashServiceDto dto);
        Task<bool> Update(Guid id, UpdateWashServiceDto dto);
        Task<bool> Delete(Guid id);
    }
}