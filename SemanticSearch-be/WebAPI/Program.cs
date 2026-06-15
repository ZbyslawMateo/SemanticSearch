using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using WebAPI.BackgroundJobs;
using WebAPI.Data;
using WebAPI.Embeddings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IEmbeddingClient, TeiEmbeddingClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Embeddings:BaseUrl"]!);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o =>
        {
            o.MigrationsAssembly("Persistence");
            o.UseVector();
        }));
builder.Services.AddHostedService<PostEmbeddingWorker>();

var app = builder.Build();
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

await db.Database.MigrateAsync();

if (app.Environment.IsDevelopment())
{
    try
    {
        await DbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/posts/search", async (
    [FromQuery] string query,
    [FromServices] AppDbContext context,
    [FromServices] IEmbeddingClient embeddingClient,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest("Query is required.");

    if (query.Length > 500)
        return Results.BadRequest("Query is too long (max 500 characters).");

    var queryEmbedding = await embeddingClient.CreateEmbeddingAsync($"Query: {query}", cancellationToken);
    var vector = new Vector(queryEmbedding);

    const double threshold = 0.10;

    var results = await context.Posts
        .AsNoTracking()
        .AsSplitQuery()
        .Where(x => x.Embedding != null)
        .Select(x => new
        {
            x.Id,
            x.Title,
            x.Description,
            Similarity = 1 - x.Embedding!.CosineDistance(vector),
            Tags = x.PostTags.Select(t => t.Tag.Value).ToList(),
            Items = x.PostItems.Select(i => new
            {
                i.Item.Id,
                i.Item.Title,
                i.Item.Description,
                Brand = i.Item.Brand != null ? i.Item.Brand.Name : null,
                Color = i.Item.Color != null ? i.Item.Color.Value : null
            }).ToList()
        })
        .Where(x => x.Similarity >= threshold)
        .OrderByDescending(x => x.Similarity)
        .Take(10)
        .ToListAsync(cancellationToken);

    if (results.Count == 0)
    {
        return Results.Ok(new
        {
            Query = query,
            Results = Array.Empty<object>(),
            Matrix = new
            {
                Rows = Array.Empty<object>(),
                Columns = Array.Empty<object>(),
                Values = Array.Empty<double[]>()
            }
        });
    }

    var matrixColumns = results
        .SelectMany(post => post.Items)
        .GroupBy(item => item.Id)
        .Select(g => g.First())
        .Select(item => new { item.Id, item.Title, item.Brand, item.Color })
        .ToList();

    var matrixRows = results
        .Select(post => new { post.Id, post.Title, post.Similarity })
        .ToList();

    var matrixValues = results
        .Select(post =>
            matrixColumns
                .Select(col => post.Items.Any(i => i.Id == col.Id) ? post.Similarity : 0d)
                .ToArray()
        )
        .ToArray();

// Flat ranked list: every post, its items and tags, with similarity
    var rankings = results.SelectMany(post =>
        {
            var rows = new List<RankingRow>
            {
                // The post itself
                new("post", post.Title, null, post.Similarity)
            };

            // Each item attached to the post (similarity derived from the post score)
            rows.AddRange(post.Items.Select(item =>
            {
                var detail = string.Join(" · ", new[] { item.Brand, item.Color }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                return new RankingRow("item", item.Title, detail, post.Similarity);
            }));

            // Each tag
            rows.AddRange(post.Tags.Select(tag => new RankingRow("tag", tag, null, post.Similarity)));

            return rows;
        })
        .DistinctBy(x => (x.Type, x.Label))
        .OrderByDescending(x => x.Similarity)
        .ToList();

    return Results.Ok(new
    {
        Query = query,
        Results = results,
        Matrix = new
        {
            Rows = matrixRows,
            Columns = matrixColumns,
            Values = matrixValues
        },
        Rankings = rankings
    });
});

app.MapGet("/", () => Results.Ok(new
{
    message = "Semantic Search API is running"
}));

app.Run();

internal sealed record RankingRow(string Type, string Label, string? Detail, double Similarity);