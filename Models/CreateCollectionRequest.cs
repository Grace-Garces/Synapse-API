using System.ComponentModel.DataAnnotations;

namespace BrainAPI.Models;

// DTO para o request de criar coleção
public class CreateCollectionRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SystemContext { get; set; } = string.Empty;
}