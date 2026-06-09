using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Infrastructure.Services;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class WashServiceController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public WashServiceController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("washes")]
        public async Task<ActionResult<List<WashServiceDto>>> GetAll()
        {
            var washes = await _serviceManager.WashServices.GetAllAsync();
            return Ok(washes);
        }

        [HttpGet("wash/{id}")]
        public async Task<ActionResult<WashServiceDto>> GetById(Guid id)
        {
            var washService = await _serviceManager.WashServices.GetByIdAsync(id);
            if (washService == null) return NotFound();
            return Ok(washService);
        }

        [HttpPost("wash")]
        public async Task<ActionResult<CreateWashServiceDto>> Create(CreateWashServiceDto dto)
        {
            var washService = await _serviceManager.WashServices.AddWashServiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = washService.Id }, washService);
        }

        [HttpPatch("wash/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateWashServiceDto dto)
        {
            var washService = await _serviceManager.WashServices.Update(id, dto);
            if (!washService) return NotFound();
            return NoContent();
        }

        [HttpDelete("wash/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var washService = await _serviceManager.WashServices.Delete(id);
            if (!washService) return NotFound();
            return NoContent();
        } 
    }
}