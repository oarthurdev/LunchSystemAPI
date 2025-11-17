using LunchSystem.Models;

namespace LunchSystem.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
}
