using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("vehicles")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vehicleService.GetAllVehiclesAsync();

            return Ok(result);
        }

        [HttpPost("vehicle")]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Dữ liệu xe gửi lên không hợp lệ rồi ní ơi!");
            }

            var result = await _vehicleService.AddVehicleAsync(dto);

            return Ok(result);
        }

        
    }
}