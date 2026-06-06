using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.Api.Controllers;

[ApiController]
[Route("api/point-logs")]
[Authorize]
public class PointLogController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public PointLogController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<PointLogDto>>> GetAll()
    {
        var logs = await _serviceManager.PointLogService.GetAllAsync();
        return Ok(logs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PointLogDto>> GetById(Guid id)
    {
        var log = await _serviceManager.PointLogService.GetByIdAsync(id);
        if (log == null) return NotFound(new { message = "Không tìm thấy lịch sử điểm." });
        return Ok(log);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<PointLogDto>>> GetByCustomerId(Guid customerId)
    {
        var logs = await _serviceManager.PointLogService.GetByCustomerIdAsync(customerId);
        return Ok(logs);
    }

    /// <summary>
    /// Chạy tiến trình xử lý các điểm đã hết hạn (Expiry)
    /// </summary>
    [HttpPost("process-expiry")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> ProcessExpiry()
    {
        var count = await _serviceManager.PointLogService.ProcessExpiredPointsAsync();
        return Ok(new { message = $"Đã xử lý {count} mục điểm hết hạn." });
    }

    /// <summary>
    /// Xem danh sách điểm sắp hết hạn trong N ngày tới
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<PointLogDto>>> GetExpiring([FromQuery] int withinDays = 30)
    {
        var logs = await _serviceManager.PointLogService.GetExpiringPointsAsync(withinDays);
        return Ok(logs);
    }
}
