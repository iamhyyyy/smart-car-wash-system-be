using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Application.Interfaces;

namespace SmartCarWash.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "admin, manager")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy user." });
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "admin, manager")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto, [FromQuery] string role = "customer")
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(dto, role);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _userService.UpdateUserAsync(id, dto);
        if (!result) return NotFound(new { message = "Không tìm thấy user." });

        return Ok(dto);
    }

    [HttpPut("{id:guid}/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
        if (!result) return BadRequest(new { message = "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại." });

        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [HttpPut("{id:guid}/lock")]
    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Lock(Guid id)
    {
        var result = await _userService.LockUserAsync(id);
        if (!result) return NotFound(new { message = "Không tìm thấy user." });

        return Ok(new { message = "Khóa tài khoản thành công." });
    }

    [HttpPut("{id:guid}/unlock")]
    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Unlock(Guid id)
    {
        var result = await _userService.UnlockUserAsync(id);
        if (!result) return NotFound(new { message = "Không tìm thấy user." });

        return Ok(new { message = "Mở khóa tài khoản thành công." });
    }

}
