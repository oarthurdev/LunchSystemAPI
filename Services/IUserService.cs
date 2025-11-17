using LunchSystem.DTOs;

namespace LunchSystem.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(int? tenantId = null);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
}
