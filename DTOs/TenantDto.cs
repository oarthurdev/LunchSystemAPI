namespace LunchSystem.DTOs;

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
}

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
