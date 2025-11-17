namespace LunchSystem.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
