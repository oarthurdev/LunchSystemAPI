namespace LunchSystem.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public int DishId { get; set; }
}
