using BrainAPI.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace BrainAPI.Services;

// (Modelos de request/response para o Ollama)
public record OllamaGenerateRequest(string model, string prompt, bool stream = false);
public record OllamaEmbeddingRequest(string model, string prompt);
public record OllamaGenerateResponse(string response);
public record OllamaEmbeddingResponse(float[] embedding);

public class OllamaClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaSettings _settings;

    public OllamaClientService(IHttpClientFactory httpClientFactory, IOptions<OllamaSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public async Task<string> GenerateAsync(string model, string prompt)
    {
        var client = _httpClientFactory.CreateClient("Ollama");
        var request = new OllamaGenerateRequest(model, prompt);
        
        var response = await client.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        return ollamaResponse?.response ?? string.Empty;
    }

    public async Task<float[]> GetEmbeddingAsync(string model, string text)
    {
        var client = _httpClientFactory.CreateClient("Ollama");
        var request = new OllamaEmbeddingRequest(model, text);

        var response = await client.PostAsJsonAsync("/api/embeddings", request);
        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return ollamaResponse?.embedding ?? Array.Empty<float>();
    }
}