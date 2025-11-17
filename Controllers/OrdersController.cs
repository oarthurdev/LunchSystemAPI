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
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HR")]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = HttpContext.Items["TenantId"] as int?;

        if (role == UserRole.SuperAdmin.ToString())
        {
            var orders = date.HasValue 
                ? await _orderService.GetByDateAsync(date.Value)
                : await _orderService.GetAllAsync();
            return Ok(orders);
        }
        else if (role == UserRole.HR.ToString() && tenantId.HasValue)
        {
            var orders = date.HasValue
                ? await _orderService.GetByDateAsync(date.Value, tenantId.Value)
                : await _orderService.GetAllAsync(tenantId.Value);
            return Ok(orders);
        }

        return Forbid();
    }

    [HttpGet("my-order")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> GetMyOrder()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var order = await _orderService.GetTodayOrderByUserAsync(userId);

        if (order == null)
            return NotFound(new { message = "Nenhum pedido encontrado para hoje" });

        return Ok(order);
    }

    [HttpGet("can-order")]
    public async Task<IActionResult> CanOrder()
    {
        var canOrder = _orderService.CanOrderToday();
        return Ok(new { canOrder, message = canOrder ? "Você pode fazer seu pedido" : "Passou do horário de solicitação do almoço!" });
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var tenantId = int.Parse(User.FindFirst("TenantId")!.Value);

        if (!_orderService.CanOrderToday())
            return BadRequest(new { message = "Passou do horário de solicitação do almoço!" });

        var order = await _orderService.CreateAsync(userId, tenantId, request);
        if (order == null)
            return BadRequest(new { message = "Pedido já realizado hoje ou prato inválido" });

        return CreatedAtAction(nameof(GetMyOrder), order);
    }
}
