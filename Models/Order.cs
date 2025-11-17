namespace LunchSystem.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DishId { get; set; }
    public int TenantId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
