using Microsoft.EntityFrameworkCore;
using LunchSystem.Data;
using LunchSystem.DTOs;
using LunchSystem.Models;

namespace LunchSystem.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync(int? tenantId = null)
    {
        var query = _context.Users.Include(u => u.Tenant).AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(u => u.TenantId == tenantId);

        return await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                TenantId = u.TenantId,
                TenantName = u.Tenant != null ? u.Tenant.Name : null,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            TenantId = request.TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var tenant = request.TenantId.HasValue 
            ? await _context.Tenants.FindAsync(request.TenantId.Value) 
            : null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            TenantId = user.TenantId,
            TenantName = tenant?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.Include(u => u.Tenant).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return null;

        user.Name = request.Name;
        user.Email = request.Email;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
