using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class PointLogService : IPointLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PointLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PointLogDto>> GetAllAsync()
        {
            var logs = await _unitOfWork.PointLogRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PointLogDto>>(logs);
        }

        public async Task<PointLogDto?> GetByIdAsync(Guid id)
        {
            var log = await _unitOfWork.PointLogRepository.GetByIdAsync(id);
            return log == null ? null : _mapper.Map<PointLogDto>(log);
        }

        public async Task<List<PointLogDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var logs = await _unitOfWork.PointLogRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<PointLogDto>>(logs);
        }

        public async Task<PointLogDto> CreateAsync(CreatePointLogDto dto)
        {
            var log = _mapper.Map<PointLog>(dto);
            log.Id = Guid.NewGuid();
            log.CreatedAt = DateTime.UtcNow;
            log.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PointLogRepository.AddAsync(log);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PointLogDto>(log);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var log = await _unitOfWork.PointLogRepository.GetByIdAsync(id);
            if (log == null) return false;

            _unitOfWork.PointLogRepository.Delete(log);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<int> ProcessExpiredPointsAsync()
        {
            var expiredLogs = await _unitOfWork.PointLogRepository.GetExpiredPointsAsync();
            if (!expiredLogs.Any()) return 0;

            int processedCount = 0;

            foreach (var log in expiredLogs)
            {
                var profile = await _unitOfWork.CustomerProfileRepository.GetByIdAsync(log.CustomerId);
                if (profile != null)
                {
                    // Trừ điểm hết hạn
                    profile.AvailablePoints = Math.Max(0, profile.AvailablePoints - log.PointsChanged);
                    profile.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.CustomerProfileRepository.Update(profile);

                    var expireLog = new PointLog
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = log.CustomerId,
                        PointsChanged = -log.PointsChanged,
                        TransactionType = PointTransactionType.Expire,
                        BalanceAfter = profile.AvailablePoints,
                        Note = $"Điểm hết hạn từ giao dịch Earn ngày {log.CreatedAt:dd/MM/yyyy}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.PointLogRepository.AddAsync(expireLog);
                }

                // Đánh dấu log đã được xử lý bằng cách clear ExpiresAt
                log.ExpiresAt = null;
                log.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.PointLogRepository.Update(log);

                processedCount++;
            }

            await _unitOfWork.CompleteAsync();
            return processedCount;
        }

        public async Task<List<PointLogDto>> GetExpiringPointsAsync(int withinDays = 30)
        {
            var logs = await _unitOfWork.PointLogRepository.GetExpiringPointsAsync(withinDays);
            return _mapper.Map<List<PointLogDto>>(logs);
        }
    }
}
