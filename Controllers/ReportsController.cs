using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LunchSystem.Models;
using LunchSystem.Services;

namespace LunchSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("monthly")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var pdfBytes = await _reportService.GenerateMonthlyReportAsync(year, month);
        return File(pdfBytes, "application/pdf", $"Relatorio_Mensal_{year}_{month:D2}.pdf");
    }

    [HttpGet("daily")]
    [Authorize(Roles = "SuperAdmin,HR")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = HttpContext.Items["TenantId"] as int?;

        if (role == UserRole.HR.ToString() && !tenantId.HasValue)
            return Forbid();

        var pdfBytes = role == UserRole.SuperAdmin.ToString()
            ? await _reportService.GenerateDailyReportForTenantAsync(tenantId ?? 0, date)
            : await _reportService.GenerateDailyReportForTenantAsync(tenantId!.Value, date);

        return File(pdfBytes, "application/pdf", $"Relatorio_Diario_{date:yyyy-MM-dd}.pdf");
    }
}
