using System.ComponentModel.DataAnnotations;

namespace BrainAPI.Models; // Ou qualquer que seja seu namespace

public class QueryRequest
{
    // A propriedade CollectionName foi REMOVIDA.

    [Required]
    public string Question { get; set; } = string.Empty;
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new List<string>();
}