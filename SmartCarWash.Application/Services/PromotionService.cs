using AutoMapper; // 1. NHỚ THÊM THƯ VIỆN NÀY NHA CẬU
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PromotionDto>> GetAllAsync()
        {
            var promotions = await _unitOfWork.PromotionRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }
        public async Task<PromotionDto?> GetByIdAsync(Guid id)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(id);

            return promotion == null ? null : _mapper.Map<PromotionDto>(promotion);
        }

        public async Task<PromotionDto> AddPromotionAsync(CreatePromotionDto dto)
        {
            var promotion = _mapper.Map<Promotion>(dto);
            promotion.Id = Guid.NewGuid();
            await _unitOfWork.PromotionRepository.AddAsync(promotion);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PromotionDto>(promotion);
        }

        public async Task<bool> Update(Guid id, UpdatePromotionDto dto)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(id);
            if (promotion == null) return false;

            _mapper.Map(dto, promotion);

            promotion.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.PromotionRepository.Update(promotion);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(id);
            if (promotion == null) return false;

            promotion.IsActive = false;

            _unitOfWork.PromotionRepository.Update(promotion);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}