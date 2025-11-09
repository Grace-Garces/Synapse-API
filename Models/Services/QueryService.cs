using BrainAPI.Data;
using BrainAPI.Models;
using BrainAPI.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BrainAPI.Services;

public class QueryService
{
    private readonly DataAiContext _context;
    private readonly OllamaClientService _ollamaClient;
    private readonly OllamaSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor; 

    public QueryService(DataAiContext context,
                        OllamaClientService ollamaClient,
                        IOptions<OllamaSettings> settings,
                        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _ollamaClient = ollamaClient;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    // --- ASSINATURA ATUALIZADA ---
    public async Task<QueryResponse> AnswerQueryAsync(int collectionId, string systemContext, string question)
    {
        var userId = _httpContextAccessor.HttpContext?.User?
                       .FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Usuário não autenticado.");
        }

        var questionEmbedding = await _ollamaClient.GetEmbeddingAsync(_settings.EmbeddingModel, question);

        // Passa o ID do usuário para o filtro de segurança
        var relevantChunks = await FindRelevantChunksAsync(collectionId, questionEmbedding, userId, topK: 10);

        if (!relevantChunks.Any())
        {
            return new QueryResponse { Answer = "Não encontrei dados sobre isso nessa coleção.", Sources = new() };
        }

        string contextText = string.Join("\n---\n", relevantChunks.Select(c => c.OriginalText));
        
        // --- PROMPT ATUALIZADO ---
        // Usa o systemContext dinâmico da coleção
        string ragPrompt = $@"
{systemContext}

Baseando-se SOMENTE no contexto fornecido abaixo, responda à pergunta do usuário.
Se a resposta não estiver no contexto, diga 'Não consigo responder com base nos dados fornecidos'.

[CONTEXTO]
{contextText}
[/CONTEXTO]

[PERGUNTA]
{question}

[RESPOSTA]
";
        var answer = await _ollamaClient.GenerateAsync(_settings.RagModel, ragPrompt);

        return new QueryResponse
        {
            Answer = answer,
            Sources = relevantChunks.Select(c => c.OriginalText).ToList()
        };
    }

    // --- ASSINATURA ATUALIZADA ---
    private async Task<List<DataChunk>> FindRelevantChunksAsync(int collectionId, float[] queryEmbedding, string userId, int topK)
    {
        // --- FILTRO ATUALIZADO ---
        var allChunks = await _context.DataChunks
            .Where(c => c.CollectionId == collectionId && c.UserId == userId) // Filtra pelo ID da coleção
            .ToListAsync();

        allChunks.ForEach(DataAiContext.DeserializeEmbedding);

        var scoredChunks = allChunks
            .Select(chunk => new
            {
                Chunk = chunk,
                Similarity = CosineSimilarity(queryEmbedding, chunk.Embedding)
            })
            .Where(x => x.Similarity > 0.5) 
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();

        return scoredChunks;
    }

    private double CosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length != v2.Length)
            throw new ArgumentException("Vetores devem ter o mesmo tamanho");
        double dotProduct = 0.0;
        double mag1 = 0.0;
        double mag2 = 0.0;
        for (int i = 0; i < v1.Length; i++)
        {
            dotProduct += v1[i] * v2[i];
            mag1 += Math.Pow(v1[i], 2);
            mag2 += Math.Pow(v2[i], 2);
        }
        mag1 = Math.Sqrt(mag1);
        mag2 = Math.Sqrt(mag2);
        if (mag1 == 0 || mag2 == 0) return 0;
        return dotProduct / (mag1 * mag2);
    }
}