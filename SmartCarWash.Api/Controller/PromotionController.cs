using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Infrastructure.Services;

namespace SmartCarWash.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class PromotionController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public PromotionController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("promotions")]
        public async Task<ActionResult<List<PromotionDto>>> GetAll()
        {
            var promotions = await _serviceManager.PromotionService.GetAllAsync();
            return Ok(promotions);
        }

        [HttpGet("promotion/{id}")]
        public async Task<ActionResult<PromotionDto>> GetById(Guid id)
        {
            var promotion = await _serviceManager.PromotionService.GetByIdAsync(id);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        [HttpPost("promotion")]
        public async Task<ActionResult<CreatePromotionDto>> Create(CreatePromotionDto dto)
        {
            var promotion = await _serviceManager.PromotionService.AddPromotionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = promotion.Id }, promotion);
        }

        [HttpPatch("promotion/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdatePromotionDto dto)
        {
            var promotion = await _serviceManager.PromotionService.Update(id, dto);
            if (!promotion) return NotFound();
            return NoContent();
        }

        [HttpDelete("promotion/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var promotion = await _serviceManager.PromotionService.Delete(id);
            if (!promotion) return NotFound();
            return NoContent();
        }
    }
}