using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.Api.Controllers;

[ApiController]
[Route("api/customer-profiles")]
[Authorize]
public class CustomerProfileController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public CustomerProfileController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<CustomerProfileDto>>> GetAll()
    {
        var profiles = await _serviceManager.CustomerProfileService.GetAllAsync();
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerProfileDto>> GetById(Guid id)
    {
        var profile = await _serviceManager.CustomerProfileService.GetByIdAsync(id);
        if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ khách hàng." });
        return Ok(profile);
    }

    [HttpGet("tier/{tierId:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<CustomerProfileDto>>> GetByTierId(Guid tierId)
    {
        var profiles = await _serviceManager.CustomerProfileService.GetByTierIdAsync(tierId);
        return Ok(profiles);
    }

    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<CustomerProfileDto>> Create([FromBody] CreateCustomerProfileDto dto)
    {
        var profile = await _serviceManager.CustomerProfileService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerProfileDto dto)
    {
        var result = await _serviceManager.CustomerProfileService.UpdateAsync(id, dto);
        if (!result) return NotFound(new { message = "Không tìm thấy hồ sơ." });

        return Ok(dto);
    }


    /// <summary>
    /// Đổi điểm để lấy ưu đãi
    /// </summary>
    [HttpPost("{id:guid}/redeem-points")]
    public async Task<IActionResult> RedeemPoints(Guid id, [FromQuery] int points, [FromQuery] string note)
    {
        if (points <= 0) return BadRequest(new { message = "Số điểm phải lớn hơn 0." });

        var result = await _serviceManager.CustomerProfileService.RedeemPointsAsync(id, points, note);
        if (!result) return BadRequest(new { message = "Đổi điểm thất bại. Vui lòng kiểm tra lại số dư." });

        return Ok(new { message = "Đổi điểm thành công." });
    }

    /// <summary>
    /// Thêm điểm thủ công cho khách hàng
    /// </summary>
    [HttpPost("{id:guid}/add-points")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> AddPoints(Guid id, [FromQuery] int points, [FromQuery] string note, [FromQuery] Guid? bookingId = null)
    {
        if (points <= 0) return BadRequest(new { message = "Số điểm phải lớn hơn 0." });

        var result = await _serviceManager.CustomerProfileService.AddPointsAsync(id, points, note, bookingId);
        if (!result) return NotFound(new { message = "Không tìm thấy hồ sơ khách hàng." });

        return Ok(new { message = "Cộng điểm thành công." });
    }
}
