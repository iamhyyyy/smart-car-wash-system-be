using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerProfileDto>> GetAllAsync()
        {
            var profiles = await _unitOfWork.CustomerProfileRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<CustomerProfileDto>>(profiles);
        }

        public async Task<CustomerProfileDto?> GetByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdWithDetailsAsync(id);
            return profile == null ? null : _mapper.Map<CustomerProfileDto>(profile);
        }

        public async Task<IEnumerable<CustomerProfileDto>> GetByTierIdAsync(Guid tierId)
        {
            var profiles = await _unitOfWork.CustomerProfileRepository.GetByTierIdAsync(tierId);
            return _mapper.Map<IEnumerable<CustomerProfileDto>>(profiles);
        }

        public async Task<CustomerProfileDto> CreateAsync(CreateCustomerProfileDto dto)
        {
            var profile = _mapper.Map<CustomerProfile>(dto);
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.LastTierReviewDate = DateTime.UtcNow;

            await _unitOfWork.CustomerProfileRepository.AddAsync(profile);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CustomerProfileDto>(profile);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCustomerProfileDto dto)
        {
            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            _mapper.Map(dto, profile);
            profile.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.CustomerProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            _unitOfWork.CustomerProfileRepository.Delete(profile);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RedeemPointsAsync(Guid customerId, int pointsToRedeem, string note)
        {
            if (pointsToRedeem <= 0) return false;

            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdAsync(customerId);
            if (profile == null || profile.AvailablePoints < pointsToRedeem) return false;

            profile.AvailablePoints -= pointsToRedeem;
            profile.UpdatedAt = DateTime.UtcNow;

            var pointLog = new PointLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                PointsChanged = -pointsToRedeem,
                TransactionType = PointTransactionType.Redeem,
                BalanceAfter = profile.AvailablePoints,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.CustomerProfileRepository.Update(profile);
            await _unitOfWork.PointLogRepository.AddAsync(pointLog);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> AddPointsAsync(Guid customerId, int points, string note, Guid? bookingId = null)
        {
            if (points <= 0) return false;

            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdAsync(customerId);
            if (profile == null) return false;

            profile.AvailablePoints += points;
            profile.LifetimePoints += points;
            profile.UpdatedAt = DateTime.UtcNow;

            var pointLog = new PointLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                BookingId = bookingId,
                PointsChanged = points,
                TransactionType = PointTransactionType.Earn,
                BalanceAfter = profile.AvailablePoints,
                Note = note,
                ExpiresAt = DateTime.UtcNow.AddYears(1), // Hết hạn sau 12 tháng
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.CustomerProfileRepository.Update(profile);
            await _unitOfWork.PointLogRepository.AddAsync(pointLog);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
