using AutoMapper; // 1. NHỚ THÊM THƯ VIỆN NÀY NHA CẬU
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class WashServices : IWashService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WashServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WashServiceDto>> GetAllAsync()
        {
            var washServices = await _unitOfWork.WashServiceRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<WashServiceDto>>(washServices);
        }
        public async Task<WashServiceDto?> GetByIdAsync(Guid id)
        {
            var washService = await _unitOfWork.WashServiceRepository.GetByIdAsync(id);

            return washService == null ? null : _mapper.Map<WashServiceDto>(washService);
        }

        public async Task<WashServiceDto> AddWashServiceAsync(CreateWashServiceDto dto)
        {
            var washService = _mapper.Map<WashService>(dto);
            washService.Id = Guid.NewGuid();
            await _unitOfWork.WashServiceRepository.AddAsync(washService);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<WashServiceDto>(washService);
        }

        public async Task<bool> Update(Guid id, UpdateWashServiceDto dto)
        {
            var washService = await _unitOfWork.WashServiceRepository.GetByIdAsync(id);
            if (washService == null) return false;

            _mapper.Map(dto, washService);

            washService.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.WashServiceRepository.Update(washService);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var washService = await _unitOfWork.WashServiceRepository.GetByIdAsync(id);
            if (washService == null) return false;

            washService.IsActive = false;

            _unitOfWork.WashServiceRepository.Update(washService);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}