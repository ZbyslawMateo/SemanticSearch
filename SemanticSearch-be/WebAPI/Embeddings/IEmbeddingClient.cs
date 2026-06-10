namespace WebAPI.Embeddings;

public interface IEmbeddingClient
{
    Task<float[]> CreateEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}