using Microsoft.EntityFrameworkCore;
using LunchSystem.Data;
using LunchSystem.DTOs;
using LunchSystem.Models;

namespace LunchSystem.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public OrderService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<OrderDto>> GetAllAsync(int? tenantId = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Dish)
            .Include(o => o.Tenant)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(o => o.TenantId == tenantId);

        return await query
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User.Name,
                DishId = o.DishId,
                DishName = o.Dish.Name,
                TenantId = o.TenantId,
                TenantName = o.Tenant.Name,
                OrderDate = o.OrderDate,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<OrderDto>> GetByDateAsync(DateTime date, int? tenantId = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Dish)
            .Include(o => o.Tenant)
            .Where(o => o.OrderDate.Date == date.Date);

        if (tenantId.HasValue)
            query = query.Where(o => o.TenantId == tenantId);

        return await query
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User.Name,
                DishId = o.DishId,
                DishName = o.Dish.Name,
                TenantId = o.TenantId,
                TenantName = o.Tenant.Name,
                OrderDate = o.OrderDate,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrderDto?> GetTodayOrderByUserAsync(int userId)
    {
        var today = DateTime.UtcNow.Date;
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Dish)
            .Include(o => o.Tenant)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderDate.Date == today);

        if (order == null)
            return null;

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User.Name,
            DishId = order.DishId,
            DishName = order.Dish.Name,
            TenantId = order.TenantId,
            TenantName = order.Tenant.Name,
            OrderDate = order.OrderDate,
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<OrderDto?> CreateAsync(int userId, int tenantId, CreateOrderRequest request)
    {
        if (!CanOrderToday())
            return null;

        var today = DateTime.UtcNow.Date;
        var existingOrder = await _context.Orders
            .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderDate.Date == today);

        if (existingOrder != null)
            return null;

        var dish = await _context.Dishes.FindAsync(request.DishId);
        if (dish == null || !dish.IsActive)
            return null;

        var order = new Order
        {
            UserId = userId,
            DishId = request.DishId,
            TenantId = tenantId,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        var tenant = await _context.Tenants.FindAsync(tenantId);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = user!.Name,
            DishId = order.DishId,
            DishName = dish.Name,
            TenantId = order.TenantId,
            TenantName = tenant!.Name,
            OrderDate = order.OrderDate,
            CreatedAt = order.CreatedAt
        };
    }

    public bool CanOrderToday()
    {
        var deadlineHour = _configuration.GetValue<int>("AppSettings:OrderDeadlineHour");
        var timezone = _configuration.GetValue<string>("AppSettings:Timezone") ?? "America/Sao_Paulo";
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

        return localTime.Hour < deadlineHour;
    }
}
