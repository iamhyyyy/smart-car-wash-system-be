using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;

namespace SmartCarWash.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        private string _customMessage = "Thank you for submitting your review of our service..";
        private string _headerColor = "#4FAA5B";

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<IEnumerable<FeedbackDto>> GetAllAsync()
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
        }
        public async Task<FeedbackDto?> GetByIdAsync(Guid id)
        {
            var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
            return feedback == null ? null : _mapper.Map<FeedbackDto>(feedback);
        }

        public async Task<List<FeedbackDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<FeedbackDto>>(feedbacks);
        }

        public async Task<int> CountByCustomerIdAsync(Guid customerId)
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetByCustomerIdAsync(customerId);
            return feedbacks.Count;
        }

        public async Task<FeedbackDto> AddFeedbackAsync(CreateFeedbackDto dto)
        {
            var feedback = _mapper.Map<Feedback>(dto);
            feedback.Id = Guid.NewGuid();
            await _unitOfWork.FeedbackRepository.AddAsync(feedback);
            await _unitOfWork.CompleteAsync();

            var fullFeedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedback.Id);
            if (fullFeedback != null && fullFeedback.Customer != null)
            {
                _customMessage = "Thank you for taking the time to review your experience at Smart Car Wash! Your feedback helps us improve our services every day.";
                _headerColor = "#2B88D9";
                var emailBody = WriteEmailContent(fullFeedback);
                await _emailService.SendEmailAsync(fullFeedback.Customer.User.Email!, "Eco Prime Hub - Feedback Submission Confirmed", emailBody);
                //await _emailService.SendEmailAsync("huyndse184016@fpt.edu.vn", "Smart Car Wash - Feedback Submission Confirmed", emailBody);
            }

            return _mapper.Map<FeedbackDto>(feedback);
        }

        public async Task<bool> Update(Guid id, UpdateFeedbackDto dto)
        {
            var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
            if (feedback == null) return false;

            _mapper.Map(dto, feedback);

            feedback.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.FeedbackRepository.Update(feedback);
            await _unitOfWork.CompleteAsync();

            var fullFeedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
            if (fullFeedback != null && fullFeedback.Customer != null)
            {
                _customMessage = "We have received the update to your feedback. Thank you for your continued support and for helping us grow.";
                _headerColor = "#E67E22";
                var emailBody = WriteEmailContent(fullFeedback);
                await _emailService.SendEmailAsync(fullFeedback.Customer.User.Email!, "Eco Prime Hub - Feedback Successfully Updated", emailBody);
                //await _emailService.SendEmailAsync("huyndse184016@fpt.edu.vn", "Smart Car Wash - Feedback Successfully Updated", emailBody);
            }

            return true;
        }

        private string WriteEmailContent(Feedback feedback)
        {
            string stars = string.Concat(Enumerable.Repeat("⭐", feedback.Rating));

            string customerName = "Valued Customer";
            if (feedback.Customer != null && feedback.Customer.User != null)
            {
                customerName = $"{feedback.Customer.User.FirstName} {feedback.Customer.User.LastName}".Trim();

                if (string.IsNullOrEmpty(customerName)) customerName = "Customer";
            }

            string emailBody = $@"
<div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #eee; border-radius: 15px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
    <div style='background-color: #2B88D9; color: white; padding: 30px; text-align: center;'>
        <h1 style='margin: 0; font-size: 24px;'>ECO PRIME HUB</h1>
        <p style='margin: 5px 0 0 0; opacity: 0.9;'>Love Your Journey, Wash Your Car</p>
    </div>

    <div style='padding: 30px;'>
        <h2 style='color: #1A5276; margin-top: 0;'>Dear {customerName},</h2>
        <p style='font-size: 16px;'>{_customMessage}</p>
        
        <div style='background-color: #f8fbfb; padding: 20px; border-left: 4px solid {_headerColor}; border-radius: 8px; margin: 25px 0;'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; color: #666;'><strong>Booking ID:</strong></td>
                    <td style='padding: 8px 0; text-align: right; font-family: monospace;'>{feedback.BookingId}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #666;'><strong>Rating:</strong></td>
                    <td style='padding: 8px 0; text-align: right; font-size: 16px;'>{stars} ({feedback.Rating}/5)</td>
                </tr>
                <tr>
                    <td style='padding: 10px 0 8px 0; border-top: 1px solid #eee; color: #666;'><strong>Your Comment:</strong></td>
                    <td style='padding: 10px 0 8px 0; text-align: right; border-top: 1px solid #eee; font-style: italic; color: #555;'>
                        ""{feedback.Comment ?? "No additional comments"}""
                    </td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; border-top: 1px solid #eee; color: #666;'><strong>Date & Time:</strong></td>
                    <td style='padding: 8px 0; text-align: right; border-top: 1px solid #eee; font-size: 13px;'>{feedback.UpdatedAt:MM/dd/yyyy HH:mm}</td>
                </tr>
            </table>
        </div>

        <div style='text-align: center; margin-top: 30px;'>
            <a href='https://smartcarwash.com/profile/feedbacks' style='background-color: #2B88D9; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Manage Your Feedbacks</a>
        </div>
    </div>

    <div style='background-color: #f4f4f4; padding: 20px; text-align: center; font-size: 12px; color: #999;'>
        <p>This is an automated email from the Smart Car Wash Management System.</p>
        <p><strong>Eco Prime Hub</strong><br>Ho Chi Minh City, Vietnam | Hotline: 1900-CAR-WASH</p>
    </div>
</div>";

            return emailBody;
        }
    }
}