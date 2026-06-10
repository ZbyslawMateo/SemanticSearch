using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace WebAPI.Embeddings;

public class TeiEmbeddingClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TeiEmbeddingClient> logger) : IEmbeddingClient
{
    private readonly string _model =
        configuration["Embeddings:Model"] ?? "jinaai/jina-embeddings-v5-text-nano-retrieval";

    public async Task<float[]> CreateEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _model,
            input = input
        };

        var preview = input.Length > 300 ? input[..300] + "..." : input;
        logger.LogInformation(
            "Embedding request starting. Model={Model}, InputLength={InputLength}, Preview={Preview}",
            _model, input.Length, preview);

        var sw = Stopwatch.StartNew();

        using var response = await httpClient.PostAsJsonAsync("/embeddings", payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogInformation(
            "Embedding response received. StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, BodyPreview={BodyPreview}",
            (int)response.StatusCode,
            sw.ElapsedMilliseconds,
            body.Length > 500 ? body[..500] + "..." : body);

        response.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<EmbeddingResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result?.Data is null || result.Data.Count == 0 || result.Data[0].Embedding.Length == 0)
        {
            logger.LogError("Embedding response was empty. RawBody={RawBody}", body);
            throw new InvalidOperationException("Embedding response was empty.");
        }

        logger.LogInformation("Embedding parsed successfully. Dimensions={Dimensions}", result.Data[0].Embedding.Length);
        return result.Data[0].Embedding;
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; } = [];
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}