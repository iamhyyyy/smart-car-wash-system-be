using AutoMapper;
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

        public async Task<Promotion> ValidateEligibilityAsync(Guid promoId, CustomerProfile customer)
        {
            var promo = await _unitOfWork.PromotionRepository.GetByIdAsync(promoId)
                ?? throw new InvalidOperationException("Không tìm thấy khuyến mãi.");

            if (!promo.IsActive)
                throw new InvalidOperationException("Khuyến mãi không còn hiệu lực.");

            var now = DateTime.UtcNow;
            if (now < promo.ValidFrom || now > promo.ValidTo)
                throw new InvalidOperationException("Khuyến mãi không nằm trong thời gian áp dụng.");

            if (promo.MinTierId.HasValue)
            {
                var minTier = await _unitOfWork.TierRepository.GetByIdAsync(promo.MinTierId.Value)
                    ?? throw new InvalidOperationException("Hạng thành viên yêu cầu của khuyến mãi không hợp lệ.");

                var customerTier = customer.CurrentTier
                    ?? await _unitOfWork.TierRepository.GetByIdAsync(customer.CurrentTierId);

                if (customerTier == null || customerTier.PriorityLevel < minTier.PriorityLevel)
                    throw new InvalidOperationException($"Khuyến mãi chỉ áp dụng cho hạng {minTier.Name} trở lên.");
            }

            if (promo.MaxUsesTotal.HasValue && promo.CurrentUses >= promo.MaxUsesTotal.Value)
                throw new InvalidOperationException("Khuyến mãi đã hết lượt sử dụng.");

            var customerUses = await _unitOfWork.BookingRepository.CountPromoUsagesByCustomerAsync(promoId, customer.Id);
            if (customerUses >= promo.MaxUsesPerCustomer)
                throw new InvalidOperationException("Bạn đã sử dụng hết lượt khuyến mãi này.");

            if (promo.PointsCost > 0 && customer.AvailablePoints < promo.PointsCost)
                throw new InvalidOperationException("Không đủ điểm để đổi khuyến mãi này.");

            return promo;
        }
    }
}