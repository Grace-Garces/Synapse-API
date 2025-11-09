using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrainAPI.Data;

public class DataChunk
{
    [Key]
    public int Id { get; set; }

    // --- REMOVIDO ---
    // [Required]
    // public string CollectionName { get; set; } = string.Empty;

    // --- ADICIONADO ---
    [Required]
    public int CollectionId { get; set; } // FK para a nova tabela

    [ForeignKey("CollectionId")]
    public virtual Collection Collection { get; set; } = null!;
    // --- FIM DA MUDANÇA ---

    [Required]
    public string OriginalText { get; set; } = string.Empty;

    [NotMapped]
    public float[] Embedding { get; set; } = Array.Empty<float>();

    [Required]
    public string EmbeddingJson { get; set; } = string.Empty; 

    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;
}