using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Infrastructure.Services;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclesController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IEmailService _emailService;

        public VehiclesController(IServiceManager serviceManager, IEmailService emailService)
        {
            _serviceManager = serviceManager;
            _emailService = emailService;
        }

        [HttpGet("vehicles")]
        public async Task<ActionResult<List<VehicleDto>>> GetAll()
        {
            var pets = await _serviceManager.VehicleService.GetAllAsync();
            return Ok(pets);
        }

        [HttpGet("vehicle/{id}")]
        public async Task<ActionResult<VehicleDto>> GetById(Guid id)
        {
            var vehicle = await _serviceManager.VehicleService.GetByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpGet("vehicles/customer/{customerId}")]
        public async Task<ActionResult<List<VehicleDto>>> GetByCustomerId(Guid customerId)
        {
            var vehicles = await _serviceManager.VehicleService.GetByCustomerIdAsync(customerId);
            return Ok(vehicles);
        }

        [HttpGet("vehicles/count/customer/{customerId}")]
        public async Task<ActionResult<int>> CountByCustomerId(Guid customerId)
        {
            var count = await _serviceManager.VehicleService.CountByCustomerIdAsync(customerId);
            return Ok(count);
        }

        [HttpPost("vehicle")]
        public async Task<ActionResult<CreateVehicleDto>> Create(CreateVehicleDto dto)
        {
            var vehicle = await _serviceManager.VehicleService.AddVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPatch("vehicle/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateVehicleDto dto)
        {
            var pet = await _serviceManager.VehicleService.Update(id, dto);
            if (!pet) return NotFound();
            return NoContent();
        }

        [HttpGet("test")]
        public async Task<IActionResult> SendTest()
        {
            await _emailService.SendEmailAsync(
                "huyndse184016@fpt.edu.vn",
                "Test Mail",
                "Hello from PetHub 🐶"
            );

            return Ok("Email sent");
        }
    }
}