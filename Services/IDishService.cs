using LunchSystem.DTOs;

namespace LunchSystem.Services;

public interface IDishService
{
    Task<List<DishDto>> GetAllAsync(int? tenantId = null);
    Task<List<DishDto>> GetAvailableTodayAsync(int? tenantId = null);
    Task<DishDto?> GetByIdAsync(int id);
    Task<DishDto> CreateAsync(CreateDishRequest request);
    Task<DishDto?> UpdateAsync(int id, UpdateDishRequest request);
    Task<bool> DeleteAsync(int id);
}
