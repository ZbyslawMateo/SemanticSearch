using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Pgvector;
using WebAPI.Embeddings;

namespace WebAPI.BackgroundJobs;

public sealed class PostEmbeddingWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PostEmbeddingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PostEmbeddingWorker started");

        await ProcessMissingEmbeddings(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessMissingEmbeddings(stoppingToken);
        }
    }

    private async Task ProcessMissingEmbeddings(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var embeddingClient = scope.ServiceProvider.GetRequiredService<IEmbeddingClient>();

            var posts = await db.Posts
                .Include(x => x.PostTags)
                    .ThenInclude(x => x.Tag)
                .Include(x => x.PostItems)
                    .ThenInclude(x => x.Item)
                    .ThenInclude(x => x.Brand)
                .Include(x => x.PostItems)
                    .ThenInclude(x => x.Item)
                    .ThenInclude(x => x.Color)
                .Where(x => x.Embedding == null)
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var post in posts)
            {
                var text = BuildEmbeddingText(post);
                var embedding = await embeddingClient.CreateEmbeddingAsync($"Document: {text}", stoppingToken);
                post.Embedding = new Vector(embedding);

                logger.LogInformation("Generated embedding for post {PostId} - {Title}", post.Id, post.Title);
            }

            if (posts.Count > 0)
            {
                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Saved embeddings for {Count} posts", posts.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed while processing missing embeddings");
        }
    }

    private static string BuildEmbeddingText(Persistence.Models.Post post)
    {
        var tags = string.Join(", ", post.PostTags.Select(x => x.Tag.Value));

        var items = string.Join("\n", post.PostItems.Select(x =>
        {
            var brand = x.Item.Brand?.Name;
            var color = x.Item.Color?.Value;

            return $"""
                    Item title: {x.Item.Title}
                    Item brand: {brand}
                    Item color: {color}
                    Item description: {x.Item.Description}
                    """;
        }));

        return $"""
                Post title: {post.Title}
                Post description: {post.Description}
                Tags: {tags}
                Related items:
                {items}
                """;
    }
}