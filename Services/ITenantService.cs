using LunchSystem.DTOs;

namespace LunchSystem.Services;

public interface ITenantService
{
    Task<List<TenantDto>> GetAllAsync();
    Task<TenantDto?> GetByIdAsync(int id);
    Task<TenantDto> CreateAsync(CreateTenantRequest request);
    Task<TenantDto?> UpdateAsync(int id, UpdateTenantRequest request);
    Task<bool> DeleteAsync(int id);
}
