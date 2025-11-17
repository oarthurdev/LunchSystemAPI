using Microsoft.EntityFrameworkCore;
using LunchSystem.Data;
using LunchSystem.DTOs;
using LunchSystem.Models;

namespace LunchSystem.Services;

public class DishService : IDishService
{
    private readonly AppDbContext _context;

    public DishService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DishDto>> GetAllAsync(int? tenantId = null)
    {
        var query = _context.Dishes.Include(d => d.Tenant).AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(d => d.TenantId == tenantId || d.TenantId == null);

        return await query
            .Select(d => new DishDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Category = d.Category,
                ImageUrl = d.ImageUrl,
                AvailableDate = d.AvailableDate,
                TenantId = d.TenantId,
                TenantName = d.Tenant != null ? d.Tenant.Name : "Global",
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<DishDto>> GetAvailableTodayAsync(int? tenantId = null)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.Dishes.Include(d => d.Tenant)
            .Where(d => d.IsActive && d.AvailableDate.Date == today);

        if (tenantId.HasValue)
            query = query.Where(d => d.TenantId == tenantId || d.TenantId == null);

        return await query
            .Select(d => new DishDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Category = d.Category,
                ImageUrl = d.ImageUrl,
                AvailableDate = d.AvailableDate,
                TenantId = d.TenantId,
                TenantName = d.Tenant != null ? d.Tenant.Name : "Global",
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DishDto?> GetByIdAsync(int id)
    {
        var dish = await _context.Dishes.Include(d => d.Tenant).FirstOrDefaultAsync(d => d.Id == id);
        if (dish == null)
            return null;

        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Category = dish.Category,
            ImageUrl = dish.ImageUrl,
            AvailableDate = dish.AvailableDate,
            TenantId = dish.TenantId,
            TenantName = dish.Tenant?.Name ?? "Global",
            IsActive = dish.IsActive,
            CreatedAt = dish.CreatedAt
        };
    }

    public async Task<DishDto> CreateAsync(CreateDishRequest request)
    {
        var dish = new Dish
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            AvailableDate = request.AvailableDate,
            TenantId = request.TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Dishes.Add(dish);
        await _context.SaveChangesAsync();

        var tenant = request.TenantId.HasValue 
            ? await _context.Tenants.FindAsync(request.TenantId.Value) 
            : null;

        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Category = dish.Category,
            ImageUrl = dish.ImageUrl,
            AvailableDate = dish.AvailableDate,
            TenantId = dish.TenantId,
            TenantName = tenant?.Name ?? "Global",
            IsActive = dish.IsActive,
            CreatedAt = dish.CreatedAt
        };
    }

    public async Task<DishDto?> UpdateAsync(int id, UpdateDishRequest request)
    {
        var dish = await _context.Dishes.Include(d => d.Tenant).FirstOrDefaultAsync(d => d.Id == id);
        if (dish == null)
            return null;

        dish.Name = request.Name;
        dish.Description = request.Description;
        dish.Category = request.Category;
        dish.ImageUrl = request.ImageUrl;
        dish.AvailableDate = request.AvailableDate;
        dish.IsActive = request.IsActive;
        dish.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Category = dish.Category,
            ImageUrl = dish.ImageUrl,
            AvailableDate = dish.AvailableDate,
            TenantId = dish.TenantId,
            TenantName = dish.Tenant?.Name ?? "Global",
            IsActive = dish.IsActive,
            CreatedAt = dish.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dish = await _context.Dishes.FindAsync(id);
        if (dish == null)
            return false;

        dish.IsActive = false;
        dish.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
