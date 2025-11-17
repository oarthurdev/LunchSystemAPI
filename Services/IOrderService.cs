using LunchSystem.DTOs;

namespace LunchSystem.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync(int? tenantId = null);
    Task<List<OrderDto>> GetByDateAsync(DateTime date, int? tenantId = null);
    Task<OrderDto?> GetTodayOrderByUserAsync(int userId);
    Task<OrderDto?> CreateAsync(int userId, int tenantId, CreateOrderRequest request);
    bool CanOrderToday();
}
