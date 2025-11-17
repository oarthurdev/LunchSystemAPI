using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LunchSystem.DTOs;
using LunchSystem.Services;

namespace LunchSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var tenant = await _tenantService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTenantRequest request)
    {
        var tenant = await _tenantService.UpdateAsync(id, request);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tenantService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
