using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LunchSystem.DTOs;
using LunchSystem.Models;
using LunchSystem.Services;

namespace LunchSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = HttpContext.Items["TenantId"] as int?;

        if (role == UserRole.SuperAdmin.ToString())
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        else if (role == UserRole.HR.ToString() && tenantId.HasValue)
        {
            var users = await _userService.GetAllAsync(tenantId.Value);
            return Ok(users);
        }

        return Forbid();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = HttpContext.Items["TenantId"] as int?;

        if (role == UserRole.HR.ToString())
        {
            if (request.Role != UserRole.Employee || request.TenantId != tenantId)
                return Forbid();
        }

        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HR")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
