using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LunchSystem.Data;

namespace LunchSystem.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task GenerateDailyReportsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tenants = await _context.Tenants.Where(t => t.IsActive).ToListAsync();

        foreach (var tenant in tenants)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Dish)
                .Where(o => o.TenantId == tenant.Id && o.OrderDate.Date == today)
                .ToListAsync();

            if (orders.Any())
            {
                var pdfBytes = GeneratePdfReport(tenant.Name, orders, today);
                var fileName = $"Pedidos_{tenant.Name.Replace(" ", "_")}_{today:yyyy-MM-dd}.pdf";
                var filePath = Path.Combine("Reports", "Daily", today.ToString("yyyy-MM-dd"), fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllBytesAsync(filePath, pdfBytes);
            }
        }
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Dish)
            .Include(o => o.Tenant)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderBy(o => o.Tenant.Name)
            .ThenBy(o => o.OrderDate)
            .ToListAsync();

        return GeneratePdfReport($"Relatório Mensal - {startDate:MMMM/yyyy}", orders, startDate);
    }

    public async Task<byte[]> GenerateDailyReportForTenantAsync(int tenantId, DateTime date)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            throw new Exception("Empresa não encontrada");

        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Dish)
            .Where(o => o.TenantId == tenantId && o.OrderDate.Date == date.Date)
            .ToListAsync();

        return GeneratePdfReport($"{tenant.Name} - {date:dd/MM/yyyy}", orders, date);
    }

    private byte[] GeneratePdfReport(string title, List<Models.Order> orders, DateTime reportDate)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text(text =>
                    {
                        text.Span("Relatório de Pedidos de Almoço\n").FontSize(20).Bold();
                        text.Span(title).FontSize(16);
                        text.Span($"\nData: {reportDate:dd/MM/yyyy}").FontSize(12);
                        text.Span($"\nTotal de Pedidos: {orders.Count}").FontSize(12);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Funcionário").Bold();
                            header.Cell().Element(CellStyle).Text("Prato").Bold();
                            header.Cell().Element(CellStyle).Text("Hora").Bold();

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold())
                                    .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        foreach (var order in orders)
                        {
                            table.Cell().Element(CellStyle).Text(order.User.Name);
                            table.Cell().Element(CellStyle).Text(order.Dish.Name);
                            table.Cell().Element(CellStyle).Text(order.CreatedAt.ToString("HH:mm"));

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .PaddingVertical(5);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
