using System.ComponentModel.DataAnnotations;

namespace Persistence.Models;

public class Item
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? BrandId { get; set; }
    public Guid? ColorId { get; set; }

    public Brand? Brand { get; set; }
    public Color? Color { get; set; }

    public ICollection<PostItem> PostItems { get; set; } = new List<PostItem>();
}