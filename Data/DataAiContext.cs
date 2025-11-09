using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BrainAPI.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BrainAPI.Data;

// Herda de IdentityDbContext
public class DataAiContext : IdentityDbContext<IdentityUser>
{
    public DataAiContext(DbContextOptions<DataAiContext> options) : base(options) { }

    // Suas tabelas
    public DbSet<DataChunk> DataChunks { get; set; }
    public DbSet<Collection> Collections { get; set; } // A nova tabela

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Mantenha esta linha primeiro

        // Configuração do DataChunk
        modelBuilder.Entity<DataChunk>(entity =>
        {
            // Ignora a propriedade [NotMapped]
            entity.Ignore(dc => dc.Embedding);
        
            // Mapeia a propriedade 'EmbeddingJson' para a coluna
            entity.Property(dc => dc.EmbeddingJson)
                  .HasColumnName("Embedding");

            // --- A CORREÇÃO ESTÁ AQUI ---
            // Define a relação entre DataChunk e User, mas quebra o ciclo de cascata
            entity.HasOne(d => d.User)               // DataChunk tem um User
                  .WithMany()                         // User tem muitos DataChunks (sem navegação)
                  .HasForeignKey(d => d.UserId)       // A chave é UserId
                  .OnDelete(DeleteBehavior.NoAction); // Impede a exclusão em cascata por este caminho
        });

        // Configuração da Collection
        modelBuilder.Entity<Collection>(entity =>
        {
            // Esta relação ESTÁ CORRETA (apagar uma coleção apaga seus chunks)
            entity.HasMany(c => c.DataChunks)
                  .WithOne(d => d.Collection)
                  .HasForeignKey(d => d.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // A relação Collection -> User usará o padrão (Cascade), que está OK
        });
    }

    //
    // Seus métodos helper
    //
    public override int SaveChanges()
    {
        SerializeEmbeddings();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SerializeEmbeddings();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SerializeEmbeddings()
    {
        var entries = ChangeTracker
            .Entries<DataChunk>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.Embedding.Length > 0)
            {
                entry.Entity.EmbeddingJson = JsonSerializer.Serialize(entry.Entity.Embedding);
            }
        }
    }
    
    public static void DeserializeEmbedding(DataChunk chunk)
    {
        if (!string.IsNullOrEmpty(chunk.EmbeddingJson))
        {
            chunk.Embedding = JsonSerializer.Deserialize<float[]>(chunk.EmbeddingJson) ?? Array.Empty<float>();
        }
    }
}