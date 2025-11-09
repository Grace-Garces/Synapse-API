namespace BrainAPI.Models.Settings;

public class OllamaSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PersonaModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
    public string RagModel { get; set; } = string.Empty;
}