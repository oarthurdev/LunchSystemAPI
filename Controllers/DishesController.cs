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
public class DishesController : ControllerBase
{
    private readonly IDishService _dishService;

    public DishesController(IDishService dishService)
    {
        _dishService = dishService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = HttpContext.Items["TenantId"] as int?;

        if (role == UserRole.SuperAdmin.ToString())
        {
            var dishes = await _dishService.GetAllAsync();
            return Ok(dishes);
        }
        else if (tenantId.HasValue)
        {
            var dishes = await _dishService.GetAllAsync(tenantId.Value);
            return Ok(dishes);
        }

        return Forbid();
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var tenantId = HttpContext.Items["TenantId"] as int?;
        var dishes = await _dishService.GetAvailableTodayAsync(tenantId);
        return Ok(dishes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        return Ok(dish);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDishRequest request)
    {
        var dish = await _dishService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = dish.Id }, dish);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDishRequest request)
    {
        var dish = await _dishService.UpdateAsync(id, request);
        if (dish == null)
            return NotFound();

        return Ok(dish);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _dishService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
