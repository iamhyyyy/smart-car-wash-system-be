using AutoMapper; // 1. NHỚ THÊM THƯ VIỆN NÀY NHA CẬU
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllAsync()
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }
        public async Task<VehicleDto?> GetByIdAsync(Guid id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(id);

            return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<List<VehicleDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<VehicleDto>>(vehicle);
        }

        public async Task<int> CountByCustomerIdAsync(Guid customerId)
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetByCustomerIdAsync(customerId);
            return vehicles.Count;
        }

        public async Task<VehicleDto> AddVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = _mapper.Map<Vehicle>(dto);
            vehicle.Id = Guid.NewGuid();
            await _unitOfWork.VehicleRepository.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<bool> Update(Guid id, UpdateVehicleDto dto)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return false;

            _mapper.Map(dto, vehicle);

            vehicle.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.VehicleRepository.Update(vehicle);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}