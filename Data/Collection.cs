using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrainAPI.Data;

public class Collection
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SystemContext { get; set; } = string.Empty; // O prompt da IA!

    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;

    // Relação: Uma coleção pode ter muitos chunks de dados
    public virtual ICollection<DataChunk> DataChunks { get; set; } = new List<DataChunk>();
}