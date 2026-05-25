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

        public async Task<VehicleDto> AddVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = _mapper.Map<Vehicle>(dto);

            await _unitOfWork.VehicleRepository.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }
    }
}