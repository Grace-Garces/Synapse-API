using System.ComponentModel.DataAnnotations;

namespace BrainAPI.Models;

public class PersonaRequest
{
    [Required]
    [MinLength(5)]
    public string Description { get; set; } = string.Empty;
}

public class PersonaResponse
{
    public string GeneratedContext { get; set; } = string.Empty;
}