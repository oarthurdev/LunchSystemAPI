using Microsoft.EntityFrameworkCore;
using LunchSystem.Data;
using LunchSystem.DTOs;
using LunchSystem.Models;

namespace LunchSystem.Services;

public class TenantService : ITenantService
{
    private readonly AppDbContext _context;

    public TenantService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantDto>> GetAllAsync()
    {
        return await _context.Tenants
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                CNPJ = t.CNPJ,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TenantDto?> GetByIdAsync(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return null;

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            CNPJ = tenant.CNPJ,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantDto> CreateAsync(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Name = request.Name,
            CNPJ = request.CNPJ,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            CNPJ = tenant.CNPJ,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantDto?> UpdateAsync(int id, UpdateTenantRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return null;

        tenant.Name = request.Name;
        tenant.CNPJ = request.CNPJ;
        tenant.IsActive = request.IsActive;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            CNPJ = tenant.CNPJ,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return false;

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
