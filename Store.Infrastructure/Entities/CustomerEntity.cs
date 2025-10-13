namespace Store.Infrastructure.Entities;

public class CustomerEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
}