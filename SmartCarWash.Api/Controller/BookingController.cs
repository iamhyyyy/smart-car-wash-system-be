using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Infrastructure.Services;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class BookingController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public BookingController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("bookings")]
        public async Task<ActionResult<List<BookingDto>>> GetAll()
        {
            var bookings = await _serviceManager.BookingService.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("booking/{id}")]
        public async Task<ActionResult<BookingDto>> GetById(Guid id)
        {
            var booking = await _serviceManager.BookingService.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("bookings/customer/{customerId}")]
        public async Task<ActionResult<List<BookingDto>>> GetByCustomerId(Guid customerId)
        {
            var bookings = await _serviceManager.BookingService.GetByCustomerIdAsync(customerId);
            return Ok(bookings);
        }

        [HttpGet("bookings/count/customer/{customerId}")]
        public async Task<ActionResult<int>> CountByCustomerId(Guid customerId)
        {
            var count = await _serviceManager.BookingService.CountByCustomerIdAsync(customerId);
            return Ok(count);
        }

        [HttpPost("booking")]
        public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
        {
            try
            {
                var booking = await _serviceManager.BookingService.AddBookingAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("booking/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateBookingDto dto)
        {
            try
            {
                var booking = await _serviceManager.BookingService.Update(id, dto);
                if (!booking) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("booking/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var booking = await _serviceManager.BookingService.Delete(id);
            if (!booking) return NotFound();
            return NoContent();
        } 
    }
}