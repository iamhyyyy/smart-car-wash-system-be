using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IPromotionService _promotionService;

        private string _customMessage = "Thank you for trusting our services.";
        private string _headerColor = "#3498DB";
        private string _backgroundColor = "#F8FBFB";

        public BookingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _promotionService = promotionService;
        }

        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            var bookings = await _unitOfWork.BookingRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<BookingDto?> GetByIdAsync(Guid id)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            return booking == null ? null : _mapper.Map<BookingDto>(booking);
        }

        public async Task<List<BookingDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var bookings = await _unitOfWork.BookingRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<int> CountByCustomerIdAsync(Guid customerId)
        {
            var bookings = await _unitOfWork.BookingRepository.GetByCustomerIdAsync(customerId);
            return bookings.Count;
        }

        public async Task<BookingDto> AddBookingAsync(CreateBookingDto dto)
        {
            var profile = await _unitOfWork.CustomerProfileRepository.GetByIdWithDetailsAsync(dto.CustomerId)
                ?? throw new InvalidOperationException("Không tìm thấy hồ sơ khách hàng.");

            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(dto.VehicleId)
                ?? throw new InvalidOperationException("Không tìm thấy phương tiện.");

            if (vehicle.CustomerId != dto.CustomerId)
                throw new InvalidOperationException("Phương tiện không thuộc về khách hàng này.");

            var service = await _unitOfWork.WashServiceRepository.GetByIdAsync(dto.ServiceId)
                ?? throw new InvalidOperationException("Không tìm thấy dịch vụ rửa xe.");

            if (!service.IsActive)
                throw new InvalidOperationException("Dịch vụ rửa xe không còn hoạt động.");

            ValidateBookingWindow(profile, dto.ScheduledTime);

            Promotion? promo = null;
            if (dto.PromoId.HasValue)
                promo = await _promotionService.ValidateEligibilityAsync(dto.PromoId.Value, profile);

            var (baseAmount, discountAmount, finalAmount, pointsRedeemed) =
                CalculateCheckout(service, promo);

            var now = DateTime.UtcNow;
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                ServiceId = dto.ServiceId,
                PromoId = dto.PromoId,
                ScheduledTime = dto.ScheduledTime,
                BaseAmount = baseAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                PointsRedeemed = pointsRedeemed,
                PointsEarned = 0,
                Status = BookingStatus.Pending,
                PaymentMethod = dto.PaymentMethod,
                StaffNotes = dto.StaffNotes,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (pointsRedeemed > 0)
                await ApplyPointsRedemptionAsync(profile, pointsRedeemed, booking.Id, $"Đổi điểm cho booking {booking.Id}");

            if (promo != null)
            {
                promo.CurrentUses++;
                promo.UpdatedAt = now;
                _unitOfWork.PromotionRepository.Update(promo);
            }

            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            var fullBooking = await _unitOfWork.BookingRepository.GetByIdAsync(booking.Id);
            if (fullBooking?.Customer?.User != null)
            {
                _customMessage = "Your booking request has been successfully recorded! Please arrive on time for the best service experience.";
                _headerColor = "#3498DB";
                _backgroundColor = "#F4F9FD";
                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!,
                    "Eco Prime Hub - Booking Confirmation",
                    WriteEmailContent(fullBooking));
            }

            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<bool> Update(Guid id, UpdateBookingDto dto)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            var previousStatus = booking.Status;
            var profile = booking.Customer
                ?? await _unitOfWork.CustomerProfileRepository.GetByIdWithDetailsAsync(booking.CustomerId)
                ?? throw new InvalidOperationException("Không tìm thấy hồ sơ khách hàng.");

            var service = booking.Service
                ?? await _unitOfWork.WashServiceRepository.GetByIdAsync(booking.ServiceId)
                ?? throw new InvalidOperationException("Không tìm thấy dịch vụ rửa xe.");

            if (dto.ScheduledTime != booking.ScheduledTime)
            {
                ValidateBookingWindow(profile, dto.ScheduledTime);
                booking.ScheduledTime = dto.ScheduledTime;
            }

            booking.CheckinTime = dto.CheckinTime;
            booking.CompletedTime = dto.CompletedTime;
            booking.Status = dto.Status;
            booking.PaymentMethod = dto.PaymentMethod;
            booking.CancelReason = dto.CancelReason;
            booking.StaffNotes = dto.StaffNotes;

            if (dto.PromoId != booking.PromoId)
            {
                Promotion? promo = null;
                if (dto.PromoId.HasValue)
                    promo = await _promotionService.ValidateEligibilityAsync(dto.PromoId.Value, profile);

                var (baseAmount, discountAmount, finalAmount, pointsRedeemed) =
                    CalculateCheckout(service, promo);

                if (pointsRedeemed > booking.PointsRedeemed)
                {
                    var additionalPoints = pointsRedeemed - booking.PointsRedeemed;
                    await ApplyPointsRedemptionAsync(profile, additionalPoints, booking.Id, $"Đổi điểm bổ sung cho booking {booking.Id}");
                }

                booking.PromoId = dto.PromoId;
                booking.BaseAmount = baseAmount;
                booking.DiscountAmount = discountAmount;
                booking.FinalAmount = finalAmount;
                booking.PointsRedeemed = pointsRedeemed;

                if (promo != null)
                {
                    promo.CurrentUses++;
                    promo.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.PromotionRepository.Update(promo);
                }
            }

            if (previousStatus != BookingStatus.Completed && booking.Status == BookingStatus.Completed)
            {
                if (!booking.CompletedTime.HasValue)
                    booking.CompletedTime = DateTime.UtcNow;

                await ProcessBookingCompletionAsync(booking, profile, service);
            }

            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.CompleteAsync();

            var fullBooking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (fullBooking?.Customer?.User != null)
            {
                if (fullBooking.Status == BookingStatus.Completed)
                {
                    _customMessage = "Great news! Your vehicle care is complete. Thank you for choosing Eco Prime Hub.";
                    _headerColor = "#2ECC71";
                    _backgroundColor = "#F5FBF7";
                }
                else if (fullBooking.Status == BookingStatus.Cancelled)
                {
                    _customMessage = $"Your booking has been cancelled. Reason: {fullBooking.CancelReason ?? "No specific reason provided"}.";
                    _headerColor = "#B85C5C";
                    _backgroundColor = "#FDF6F6";
                }
                else
                {
                    _customMessage = "Your scheduled appointment details have been updated within our management system.";
                    _headerColor = "#16A085";
                    _backgroundColor = "#F2F9F8";
                }

                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!,
                    $"Eco Prime Hub - Booking Status Update (#{fullBooking.Status})",
                    WriteEmailContent(fullBooking));
            }

            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            booking.Status = BookingStatus.Cancelled;
            booking.CancelReason = "Cancelled by customer request via system interface.";
            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.CompleteAsync();

            var fullBooking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (fullBooking?.Customer?.User != null)
            {
                _customMessage = "We are sorry to see you cancel this appointment. We hope to welcome you back on your next journey!";
                _headerColor = "#B85C5C";
                _backgroundColor = "#FDF6F6";
                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!,
                    "Eco Prime Hub - Booking Cancellation Notice",
                    WriteEmailContent(fullBooking));
            }

            return true;
        }

        private static void ValidateBookingWindow(CustomerProfile profile, DateTime scheduledTime)
        {
            var tier = profile.CurrentTier
                ?? throw new InvalidOperationException("Không tìm thấy hạng thành viên của khách hàng.");

            var now = DateTime.UtcNow;
            if (scheduledTime < now)
                throw new InvalidOperationException("Không thể đặt lịch trong quá khứ.");

            var maxAllowed = now.AddDays(tier.BookingWindowDays);
            if (scheduledTime > maxAllowed)
                throw new InvalidOperationException(
                    $"Hạng {tier.Name} chỉ được đặt lịch trước tối đa {tier.BookingWindowDays} ngày.");
        }

        private static (decimal baseAmount, decimal discountAmount, decimal finalAmount, int pointsRedeemed) CalculateCheckout(
            WashService service, Promotion? promo)
        {
            var baseAmount = service.BasePrice;
            decimal discount = 0;
            var pointsRedeemed = promo?.PointsCost ?? 0;

            if (promo != null)
            {
                discount = promo.PromoType switch
                {
                    PromoType.Discount => promo.DiscountAmount > 0
                        ? promo.DiscountAmount
                        : Math.Round(baseAmount * promo.DiscountPercent / 100m, 2),
                    PromoType.FreeWash => baseAmount,
                    PromoType.Addon => promo.DiscountAmount > 0
                        ? promo.DiscountAmount
                        : Math.Round(baseAmount * promo.DiscountPercent / 100m, 2),
                    PromoType.PointBonus => 0,
                    _ => 0
                };
            }

            discount = Math.Min(discount, baseAmount);
            var finalAmount = Math.Max(0, baseAmount - discount);
            return (baseAmount, discount, finalAmount, pointsRedeemed);
        }

        private async Task ApplyPointsRedemptionAsync(CustomerProfile profile, int points, Guid bookingId, string note)
        {
            if (points <= 0) return;

            if (profile.AvailablePoints < points)
                throw new InvalidOperationException("Không đủ điểm để áp dụng khuyến mãi.");

            profile.AvailablePoints -= points;
            profile.UpdatedAt = DateTime.UtcNow;

            var pointLog = new PointLog
            {
                Id = Guid.NewGuid(),
                CustomerId = profile.Id,
                BookingId = bookingId,
                PointsChanged = -points,
                TransactionType = PointTransactionType.Redeem,
                BalanceAfter = profile.AvailablePoints,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.CustomerProfileRepository.Update(profile);
            await _unitOfWork.PointLogRepository.AddAsync(pointLog);
        }

        private async Task ProcessBookingCompletionAsync(Booking booking, CustomerProfile profile, WashService service)
        {
            var tier = profile.CurrentTier
                ?? await _unitOfWork.TierRepository.GetByIdAsync(profile.CurrentTierId);

            var multiplier = tier?.PointMultiplier ?? 1.0m;
            var pointsEarned = (int)Math.Floor(service.PointsPerTransaction * multiplier);

            if (booking.PromoId.HasValue)
            {
                var promo = booking.Promo
                    ?? await _unitOfWork.PromotionRepository.GetByIdAsync(booking.PromoId.Value);

                if (promo?.PromoType == PromoType.PointBonus && promo.PointsCost > 0)
                    pointsEarned += promo.PointsCost;
            }

            booking.PointsEarned = pointsEarned;
            profile.TotalVisits++;
            profile.TotalSpending += booking.FinalAmount;
            profile.AvailablePoints += pointsEarned;
            profile.LifetimePoints += pointsEarned;
            profile.UpdatedAt = DateTime.UtcNow;

            var earnLog = new PointLog
            {
                Id = Guid.NewGuid(),
                CustomerId = profile.Id,
                BookingId = booking.Id,
                PointsChanged = pointsEarned,
                TransactionType = PointTransactionType.Earn,
                BalanceAfter = profile.AvailablePoints,
                Note = $"Tích điểm sau khi hoàn tất booking (x{multiplier})",
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.CustomerProfileRepository.Update(profile);
            await _unitOfWork.PointLogRepository.AddAsync(earnLog);
        }

        private string WriteEmailContent(Booking booking)
        {
            string customerName = "Valued Customer";
            if (booking.Customer?.User != null)
            {
                customerName = $"{booking.Customer.User.FirstName} {booking.Customer.User.LastName}".Trim();
                if (string.IsNullOrEmpty(customerName)) customerName = "Customer";
            }

            string vehicleInfo = booking.Vehicle != null ? $"{booking.Vehicle.Model} ({booking.Vehicle.LicensePlate})" : "Linked Vehicle Data";
            string serviceName = booking.Service != null ? booking.Service.Name : "Car Wash / Detailing Service";

            string finalAmountFormatted = booking.FinalAmount.ToString("N0") + " VND";
            string discountFormatted = booking.DiscountAmount > 0 ? $"-{booking.DiscountAmount.ToString("N0")} VND" : "0 VND";
            string pointsInfo = booking.PointsEarned > 0 ? $"+{booking.PointsEarned} pts" : "0";

            return $@"
<div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #444; max-width: 600px; margin: auto; border: 1px solid #eef2f5; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
    <div style='background-color: {_headerColor}; color: white; padding: 32px; text-align: center;'>
        <h1 style='margin: 0; font-size: 22px; letter-spacing: 1.5px; font-weight: 600;'>ECO PRIME HUB</h1>
        <p style='margin: 6px 0 0 0; opacity: 0.85; font-size: 13px; font-style: italic; letter-spacing: 0.5px;'>Love Your Journey, Wash Your Car</p>
    </div>

    <div style='padding: 30px; background-color: #ffffff;'>
        <h2 style='color: #2C3E50; margin-top: 0; font-size: 18px; font-weight: 600;'>Dear {customerName},</h2>
        <p style='font-size: 14px; color: #5c6b73; margin-bottom: 25px;'>{_customMessage}</p>
        
        <div style='background-color: {_backgroundColor}; padding: 22px; border-left: 4px solid {_headerColor}; border-radius: 8px; margin: 20px 0;'>
            <h3 style='margin-top: 0; color: #333333; font-size: 15px; border-bottom: 1px solid rgba(0,0,0,0.05); padding-bottom: 10px; font-weight: 600;'>Appointment Details</h3>
            <table style='width: 100%; border-collapse: collapse; font-size: 14px; color: #555555;'>
                <tr>
                    <td style='padding: 8px 0; color: #7f8c8d;'>Ref ID:</td>
                    <td style='padding: 8px 0; text-align: right; font-family: monospace; font-weight: bold; color: #34495E;'>#{booking.Id.ToString().Substring(0, 8).ToUpper()}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #7f8c8d;'>Vehicle:</td>
                    <td style='padding: 8px 0; text-align: right; color: #333;'>{vehicleInfo}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #7f8c8d;'>Service Class:</td>
                    <td style='padding: 8px 0; text-align: right; font-weight: 500; color: #333;'>{serviceName}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #7f8c8d;'>Scheduled Time:</td>
                    <td style='padding: 8px 0; text-align: right; font-weight: bold; color: #34495E;'>{booking.ScheduledTime:MM/dd/yyyy HH:mm}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #7f8c8d;'>Process Status:</td>
                    <td style='padding: 8px 0; text-align: right; font-weight: 600; font-size: 12px; color: white; background-color: {_headerColor}; display: inline-block; padding: 2px 8px; border-radius: 4px; text-transform: uppercase; float: right; margin-top: 4px;'>{booking.Status}</td>
                </tr>
                <tr style='border-top: 1px dashed #e0e0e0;'>
                    <td style='padding: 10px 0 6px 0; color: #7f8c8d;'>Discount Applied:</td>
                    <td style='padding: 10px 0 6px 0; text-align: right; color: #E74C3C;'>{discountFormatted}</td>
                </tr>
                <tr>
                    <td style='padding: 6px 0; color: #333; font-size: 15px; font-weight: 600;'>Grand Total:</td>
                    <td style='padding: 6px 0; text-align: right; font-size: 16px; font-weight: bold; color: #27AE60;'>{finalAmountFormatted}</td>
                </tr>
                <tr style='border-top: 1px solid rgba(0,0,0,0.05);'>
                    <td style='padding: 8px 0 4px 0; color: #7f8c8d;'>Payment Scheme:</td>
                    <td style='padding: 8px 0 4px 0; text-align: right; color: #555;'>{booking.PaymentMethod}</td>
                </tr>
                <tr>
                    <td style='padding: 4px 0 8px 0; color: #7f8c8d;'>Points Accrued:</td>
                    <td style='padding: 4px 0 8px 0; text-align: right; color: #2ECC71; font-weight: bold;'>{pointsInfo}</td>
                </tr>
            </table>
        </div>
    </div>

    <div style='background-color: #f8fafc; padding: 24px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #edf2f7;'>
        <p style='margin: 0 0 4px 0;'>This is an automated system notification from the Eco Prime Hub Booking Cluster.</p>
        <p style='margin: 0;'><strong>Eco Prime Hub</strong><br>Ho Chi Minh City, Vietnam | Hotline: 1900-CAR-WASH</p>
    </div>
</div>";
        }
    }
}
