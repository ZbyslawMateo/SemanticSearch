namespace Persistence.Models;

public class Color
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Value { get; set; } = null!;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}