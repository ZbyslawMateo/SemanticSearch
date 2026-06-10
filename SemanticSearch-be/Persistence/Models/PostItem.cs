namespace Persistence.Models;

public class PostItem
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
}