using BrainAPI.Data;
using BrainAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BrainAPI.Services;

public class DataIngestionService
{
    private readonly DataAiContext _context;
    private readonly OllamaClientService _ollamaClient;
    private readonly OllamaSettings _settings;
    // O IHttpContextAccessor não é mais necessário aqui, 
    // pois o Controller passará o userId

    public DataIngestionService(DataAiContext context,
                                OllamaClientService ollamaClient,
                                IOptions<OllamaSettings> settings)
    {
        _context = context;
        _ollamaClient = ollamaClient;
        _settings = settings.Value;
    }

    // --- ASSINATURA ATUALIZADA ---
    public async Task IngestDataAsync(int collectionId, string userId, string textContent)
    {
        if (string.IsNullOrEmpty(userId) || collectionId <= 0)
        {
            throw new InvalidOperationException("ID do usuário ou da coleção inválido.");
        }
        
        var chunks = ChunkText(textContent, 500); 

        foreach (var chunkText in chunks)
        {
            var embedding = await _ollamaClient.GetEmbeddingAsync(_settings.EmbeddingModel, chunkText);

            var dataChunk = new DataChunk
            {
                CollectionId = collectionId, // <-- MUDANÇA
                OriginalText = chunkText,
                Embedding = embedding,
                UserId = userId
            };
            _context.DataChunks.Add(dataChunk);
        }

        await _context.SaveChangesAsync();
    }

    private List<string> ChunkText(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }
        return chunks;
    }
}