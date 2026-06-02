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
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceManager.VehicleService.GetAllVehiclesAsync();

            return Ok(result);
        }

        [HttpPost("vehicle")]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Dữ liệu xe gửi lên không hợp lệ rồi ní ơi!");
            }

            var result = await _serviceManager.VehicleService.AddVehicleAsync(dto);

            return Ok(result);
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