using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class TierService : ITierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TierDto>> GetAllAsync()
        {
            var tiers = await _unitOfWork.TierRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TierDto>>(tiers);
        }

        public async Task<IEnumerable<TierDto>> GetActiveAsync()
        {
            var tiers = await _unitOfWork.TierRepository.GetActiveAsync();
            return _mapper.Map<IEnumerable<TierDto>>(tiers);
        }

        public async Task<TierDto?> GetByIdAsync(Guid id)
        {
            var tier = await _unitOfWork.TierRepository.GetByIdAsync(id);
            return tier == null ? null : _mapper.Map<TierDto>(tier);
        }

        public async Task<TierDto> CreateAsync(CreateTierDto dto)
        {
            var tier = _mapper.Map<Tier>(dto);
            tier.Id = Guid.NewGuid();
            tier.CreatedAt = DateTime.UtcNow;
            tier.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TierRepository.AddAsync(tier);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<TierDto>(tier);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTierDto dto)
        {
            var tier = await _unitOfWork.TierRepository.GetByIdAsync(id);
            if (tier == null) return false;

            _mapper.Map(dto, tier);
            tier.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TierRepository.Update(tier);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var tier = await _unitOfWork.TierRepository.GetByIdAsync(id);
            if (tier == null) return false;

            // Soft delete: đặt IsActive = false thay vì xóa thật
            tier.IsActive = false;
            tier.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TierRepository.Update(tier);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Monthly Tier Review:
        /// Lấy tất cả tiers sắp xếp theo MinPointsRequired DESC,
        /// sau đó gán tier phù hợp nhất cho từng customer dựa trên LifetimePoints.
        /// Trả về số lượng customer được upgrade/downgrade.
        /// </summary>
        public async Task<int> RunMonthlyTierReviewAsync()
        {
            var allTiers = (await _unitOfWork.TierRepository.GetActiveAsync())
                .OrderByDescending(t => t.MinPointsRequired)
                .ToList();

            var allCustomers = (await _unitOfWork.CustomerProfileRepository.GetAllWithDetailsAsync()).ToList();

            int changedCount = 0;

            foreach (var customer in allCustomers)
            {
                // Tìm tier cao nhất mà customer đủ điều kiện
                var newTier = allTiers.FirstOrDefault(t => customer.LifetimePoints >= t.MinPointsRequired);
                if (newTier == null) continue;

                if (customer.CurrentTierId != newTier.Id)
                {
                    customer.CurrentTierId = newTier.Id;
                    customer.TierUpgradedAt = DateTime.UtcNow;
                    changedCount++;
                }

                customer.LastTierReviewDate = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.CustomerProfileRepository.Update(customer);
            }

            if (changedCount > 0)
                await _unitOfWork.CompleteAsync();

            return changedCount;
        }
    }
}
