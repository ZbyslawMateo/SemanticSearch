using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace Persistence.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Vector? Embedding { get; set; }

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<PostItem> PostItems { get; set; } = new List<PostItem>();
}