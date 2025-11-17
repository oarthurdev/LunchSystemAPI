namespace LunchSystem.Services;

public interface IReportService
{
    Task GenerateDailyReportsAsync();
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month);
    Task<byte[]> GenerateDailyReportForTenantAsync(int tenantId, DateTime date);
}
