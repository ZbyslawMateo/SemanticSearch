namespace Persistence.Models;

public class Brand
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = null!;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}