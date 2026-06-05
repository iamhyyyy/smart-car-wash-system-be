using AutoMapper; // 1. NHỚ THÊM THƯ VIỆN NÀY NHA CẬU
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

        private string _customMessage = "Thank you for trusting our services.";
        private string _headerColor = "#3498DB"; // Default Theme Color (Blue)
        private string _backgroundColor = "#F8FBFB"; // Default light background

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
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
            var booking = _mapper.Map<Booking>(dto);
            booking.Id = Guid.NewGuid();
            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            // Fetch fully tracked entity with Includes (Customer, Vehicle, WashService)
            var fullBooking = await _unitOfWork.BookingRepository.GetByIdAsync(booking.Id);
            if (fullBooking != null && fullBooking.Customer != null && fullBooking.Customer.User != null)
            {
                _customMessage = "Your booking request has been successfully recorded! Please arrive on time for the best service experience.";
                _headerColor = "#3498DB"; // Dịu nhẹ, đáng tin cậy
                _backgroundColor = "#F4F9FD"; // Nền xanh dương siêu nhạt
                var emailBody = WriteEmailContent(fullBooking);

                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!, 
                    "Eco Prime Hub - Booking Confirmation", 
                    emailBody
                );
            }

            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<bool> Update(Guid id, UpdateBookingDto dto)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            _mapper.Map(dto, booking);

            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.CompleteAsync();

            var fullBooking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
            if (fullBooking != null && fullBooking.Customer != null && fullBooking.Customer.User != null)
            {
                if (fullBooking.Status == BookingStatus.Completed)
                {
                    _customMessage = "Great news! Your vehicle care is complete. Thank you for choosing Eco Prime Hub.";
                    _headerColor = "#2ECC71"; // Xanh lá cây dịu (Eco-friendly)
                    _backgroundColor = "#F5FBF7"; // Nền xám xanh lá siêu nhẹ
                }
                else if (fullBooking.Status == BookingStatus.Cancelled)
                {
                    _customMessage = $"Your booking has been cancelled. Reason: {fullBooking.CancelReason ?? "No specific reason provided"}.";
                    _headerColor = "#B85C5C"; // Màu hồng đất/đỏ lặng nhã nhặn, không gắt
                    _backgroundColor = "#FDF6F6"; // Nền hồng phấn cực nhạt mịn mắt
                }
                else
                {
                    _customMessage = "Your scheduled appointment details have been updated within our management system.";
                    _headerColor = "#16A085"; // Xanh Teal/Mint thanh lịch thay cho màu cam chói
                    _backgroundColor = "#F2F9F8";
                }

                var emailBody = WriteEmailContent(fullBooking);
                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!,
                    $"Eco Prime Hub - Booking Status Update (#{fullBooking.Status})",
                    emailBody
                );
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
            if (fullBooking != null && fullBooking.Customer != null && fullBooking.Customer.User != null)
            {
                _customMessage = "We are sorry to see you cancel this appointment. We hope to welcome you back on your next journey!";
                _headerColor = "#B85C5C"; // Màu hồng đất ấm áp, lịch sự
                _backgroundColor = "#FDF6F6";
                var emailBody = WriteEmailContent(fullBooking);

                await _emailService.SendEmailAsync(
                    fullBooking.Customer.User.Email!,
                    "Eco Prime Hub - Booking Cancellation Notice",
                    emailBody
                );
            }

            return true;
        }
        
        private string WriteEmailContent(Booking booking)
        {
            // Process customer display name safely
            string customerName = "Valued Customer";
            if (booking.Customer != null && booking.Customer.User != null)
            {
                customerName = $"{booking.Customer.User.FirstName} {booking.Customer.User.LastName}".Trim();
                if (string.IsNullOrEmpty(customerName)) customerName = "Customer";
            }

            // Safe fallbacks if related navigation entities aren't eager loaded (Included)
            string vehicleInfo = booking.Vehicle != null ? $"{booking.Vehicle.Model} ({booking.Vehicle.LicensePlate})" : "Linked Vehicle Data";
            string serviceName = booking.Service != null ? booking.Service.Name : "Car Wash / Detailing Service";

            // Format amounts and rewards
            string finalAmountFormatted = booking.FinalAmount.ToString("N0") + " VND";
            string discountFormatted = booking.DiscountAmount > 0 ? $"-{booking.DiscountAmount.ToString("N0")} VND" : "0 VND";
            string pointsInfo = booking.PointsEarned > 0 ? $"+{booking.PointsEarned} pts" : "0";

            string emailBody = $@"
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

        <div style='text-align: center; margin-top: 32px;'>
            <a href='https://smartcarwash.com/profile/bookings' style='background-color: {_headerColor}; color: white; padding: 12px 28px; text-decoration: none; border-radius: 6px; font-size: 14px; font-weight: bold; display: inline-block; box-shadow: 0 2px 6px rgba(0,0,0,0.05);'>Manage Your Appointments</a>
        </div>
    </div>

    <div style='background-color: #f8fafc; padding: 24px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #edf2f7;'>
        <p style='margin: 0 0 4px 0;'>This is an automated system notification from the Eco Prime Hub Booking Cluster.</p>
        <p style='margin: 0;'><strong>Eco Prime Hub</strong><br>Ho Chi Minh City, Vietnam | Hotline: 1900-CAR-WASH</p>
    </div>
</div>";

            return emailBody;
        }
    }
}