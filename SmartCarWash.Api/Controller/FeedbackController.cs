using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Infrastructure.Services;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class FeedbackController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        

        public FeedbackController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("feedbacks")]
        public async Task<ActionResult<List<FeedbackDto>>> GetAll()
        {
            var feedbacks = await _serviceManager.FeedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpGet("feedback/{id}")]
        public async Task<ActionResult<FeedbackDto>> GetById(Guid id)
        {
            var feedback = await _serviceManager.FeedbackService.GetByIdAsync(id);
            if (feedback == null) return NotFound();
            return Ok(feedback);
        }

        [HttpGet("feedbacks/customer/{customerId}")]
        public async Task<ActionResult<List<FeedbackDto>>> GetByCustomerId(Guid customerId)
        {
            var feedbacks = await _serviceManager.FeedbackService.GetByCustomerIdAsync(customerId);
            return Ok(feedbacks);
        }

        [HttpGet("feedbacks/count/customer/{customerId}")]
        public async Task<ActionResult<int>> CountByCustomerId(Guid customerId)
        {
            var count = await _serviceManager.FeedbackService.CountByCustomerIdAsync(customerId);
            return Ok(count);
        }

        [HttpPost("feedback")]
        public async Task<ActionResult<CreateFeedbackDto>> Create(CreateFeedbackDto dto)
        {
            var feedback = await _serviceManager.FeedbackService.AddFeedbackAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = feedback.Id }, feedback);
        }

        [HttpPatch("feedback/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateFeedbackDto dto)
        {
            var feedback = await _serviceManager.FeedbackService.Update(id, dto);
            if (!feedback) return NotFound();
            return NoContent();
        }
    }
}