using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Models;

namespace WebAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Posts.AnyAsync())
            return;

        var basePath = Path.Combine(AppContext.BaseDirectory, "Data");

        var postsPath = Path.Combine(basePath, "Posts.json");
        var itemsPath = Path.Combine(basePath, "Items.json");
        var postItemsPath = Path.Combine(basePath, "PostItems.json");
        var postTagsPath = Path.Combine(basePath, "PostTags.json");
        var tagsPath = Path.Combine(basePath, "Tags.json");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var postDtos = JsonSerializer.Deserialize<List<PostDto>>(await File.ReadAllTextAsync(postsPath), jsonOptions) ??
                       [];
        var itemDtos = JsonSerializer.Deserialize<List<ItemDto>>(await File.ReadAllTextAsync(itemsPath), jsonOptions) ??
                       [];
        var postItemDtos =
            JsonSerializer.Deserialize<List<PostItemDto>>(await File.ReadAllTextAsync(postItemsPath), jsonOptions) ??
            [];
        var postTagDtos =
            JsonSerializer.Deserialize<List<PostTagDto>>(await File.ReadAllTextAsync(postTagsPath), jsonOptions) ?? [];
        var tagDtos = JsonSerializer.Deserialize<List<TagDto>>(await File.ReadAllTextAsync(tagsPath), jsonOptions) ??
                      [];

        var nike = new Brand { Id = Guid.NewGuid(), Name = "Nike" };
        var zara = new Brand { Id = Guid.NewGuid(), Name = "Zara" };
        var hm = new Brand { Id = Guid.NewGuid(), Name = "H&M" };

        var black = new Color { Id = Guid.NewGuid(), Value = "Black" };
        var white = new Color { Id = Guid.NewGuid(), Value = "White" };
        var beige = new Color { Id = Guid.NewGuid(), Value = "Beige" };

        var posts = postDtos
            .Select(x => new Post
            {
                Id = x.PostId,
                Title = x.Title ?? string.Empty,
                Description = string.IsNullOrWhiteSpace(x.Description) ? string.Empty : x.Description
            })
            .ToList();

        var items = itemDtos
            .Select(x => new Item
            {
                Id = x.ItemId,
                Title = x.Name ?? string.Empty,
                Description = string.IsNullOrWhiteSpace(x.Description) ? string.Empty : x.Description
            })
            .ToList();

        // Distribute brands/colors across all items so every item has both,
        // instead of only the first three.
        var brands = new[] { nike, zara, hm };
        var colors = new[] { black, white, beige };

        for (var i = 0; i < items.Count; i++)
        {
            items[i].BrandId = brands[i % brands.Length].Id;
            items[i].ColorId = colors[i % colors.Length].Id;
        }

        var tags = tagDtos
            .Select(x => new Tag
            {
                Id = x.Id,
                Value = x.Name ?? string.Empty
            })
            .ToList();

        var existingPostIds = posts.Select(x => x.Id).ToHashSet();
        var existingItemIds = items.Select(x => x.Id).ToHashSet();
        var existingTagIds = tags.Select(x => x.Id).ToHashSet();

        var postItems = postItemDtos
            .Where(x => existingPostIds.Contains(x.PostId) && existingItemIds.Contains(x.ItemId))
            .Select(x => new PostItem { PostId = x.PostId, ItemId = x.ItemId })
            .ToList();

        var postTags = postTagDtos
            .Where(x => existingPostIds.Contains(x.PostId) && existingTagIds.Contains(x.TagId))
            .Select(x => new PostTag { PostId = x.PostId, TagId = x.TagId })
            .ToList();

        db.Brands.AddRange(nike, zara, hm);
        db.Colors.AddRange(black, white, beige);
        db.Posts.AddRange(posts);
        db.Items.AddRange(items);
        db.Tags.AddRange(tags);
        db.PostItems.AddRange(postItems);
        db.PostTags.AddRange(postTags);

        // Seed atomically so a mid-way failure rolls back instead of leaving
        // the database half-seeded (which the AnyAsync guard above would then skip).
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private sealed class PostDto
    {
        public Guid PostId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    private sealed class ItemDto
    {
        public Guid ItemId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private sealed class PostItemDto
    {
        public Guid PostId { get; set; }
        public Guid ItemId { get; set; }
    }

    private sealed class PostTagDto
    {
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
    }

    private sealed class TagDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}