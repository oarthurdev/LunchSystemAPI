namespace LunchSystem.DTOs;

public class DishDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime AvailableDate { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDishRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime AvailableDate { get; set; }
    public int? TenantId { get; set; }
}

public class UpdateDishRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime AvailableDate { get; set; }
    public bool IsActive { get; set; }
}
