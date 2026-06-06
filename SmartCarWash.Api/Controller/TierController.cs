using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.Api.Controllers;

[ApiController]
[Route("api/tiers")]
public class TierController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public TierController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<TierDto>>> GetAll()
    {
        var tiers = await _serviceManager.TierService.GetAllAsync();
        return Ok(tiers);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TierDto>>> GetActive()
    {
        var tiers = await _serviceManager.TierService.GetActiveAsync();
        return Ok(tiers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TierDto>> GetById(Guid id)
    {
        var tier = await _serviceManager.TierService.GetByIdAsync(id);
        if (tier == null) return NotFound(new { message = "Không tìm thấy hạng thành viên." });
        return Ok(tier);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TierDto>> Create([FromBody] CreateTierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var tier = await _serviceManager.TierService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tier.Id }, tier);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _serviceManager.TierService.UpdateAsync(id, dto);
        if (!result) return NotFound(new { message = "Không tìm thấy hạng thành viên." });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _serviceManager.TierService.DeleteAsync(id);
        if (!result) return NotFound(new { message = "Không tìm thấy hạng thành viên." });

        return NoContent();
    }

    /// <summary>
    /// Chạy tiến trình review tự động: nâng/hạ hạng thành viên dựa vào LifetimePoints
    /// </summary>
    [HttpPost("run-monthly-review")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> RunMonthlyReview()
    {
        var changedCount = await _serviceManager.TierService.RunMonthlyTierReviewAsync();
        return Ok(new { message = $"Đã chạy review hạng thành viên thành công. Số khách hàng được cập nhật hạng: {changedCount}." });
    }
}
