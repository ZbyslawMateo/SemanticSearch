using System.ComponentModel.DataAnnotations;

namespace Persistence.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    [MaxLength(30)]
    public string Value { get; set; } = string.Empty;

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}