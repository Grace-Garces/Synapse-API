using BrainAPI.Data;
using BrainAPI.Models;
using BrainAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Certifique-se que este 'using' está aqui
using System.Security.Claims;
using System.Text;

namespace BrainAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/collections")]
public class CollectionController : ControllerBase
{
    private readonly DataAiContext _context;
    private readonly DataIngestionService _ingestionService;
    private readonly QueryService _queryService;
    private readonly FileProcessorService _fileProcessorService;

    public CollectionController(
        DataAiContext context,
        DataIngestionService ingestionService,
        QueryService queryService,
        FileProcessorService fileProcessorService)
    {
        _context = context;
        _ingestionService = ingestionService;
        _queryService = queryService;
        _fileProcessorService = fileProcessorService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Endpoint para criar a coleção
    [HttpPost]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        var userId = GetUserId();
        var collection = new Collection
        {
            Name = request.Name,
            SystemContext = request.SystemContext,
            UserId = userId
        };

        _context.Collections.Add(collection);
        await _context.SaveChangesAsync();

        return Ok(new { collection.Id, collection.Name });
    }

    // Endpoint para listar coleções do usuário
    [HttpGet]
    public async Task<IActionResult> GetCollections()
    {
        var userId = GetUserId();
        var collections = await _context.Collections
            .Where(c => c.UserId == userId)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        
        return Ok(collections);
    }

    // --- CORREÇÃO ADICIONADA AQUI ---
    // Endpoint para buscar detalhes de UMA coleção
    // (O seu chat-collection.js PRECISA disso)
    [HttpGet("{collectionId}")]
    public async Task<IActionResult> GetCollectionDetails(int collectionId)
    {
        var userId = GetUserId();
        var collection = await _context.Collections
            .Where(c => c.Id == collectionId && c.UserId == userId)
            .Select(c => new 
            { 
                c.Id, 
                c.Name, 
                c.SystemContext,
                // Conta quantos chunks existem para esta coleção
                DataChunksCount = c.DataChunks.Count() 
            })
            .FirstOrDefaultAsync();

        if (collection == null) return NotFound("Coleção não encontrada.");

        return Ok(collection);
    }
    // --- FIM DA CORREÇÃO ---
    
    // Endpoint para upload de arquivos
    [HttpPost("{collectionId}/upload")]
    public async Task<IActionResult> UploadFiles(int collectionId, [FromForm] List<IFormFile> files)
    {
        var userId = GetUserId();
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return NotFound("Coleção não encontrada.");
        if (files == null || !files.Any() || files.Count > 5) return BadRequest("Envie de 1 a 5 arquivos.");

        var combinedText = new StringBuilder();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            var text = await _fileProcessorService.ExtractTextAsync(file);
            combinedText.AppendLine($"--- Início {file.FileName} ---");
            combinedText.AppendLine(text);
            combinedText.AppendLine($"--- Fim {file.FileName} ---\n");
        }
        
        await _ingestionService.IngestDataAsync(collection.Id, userId, combinedText.ToString());

        return Ok(new { Message = $"{files.Count} arquivos ingeridos na coleção '{collection.Name}'." });
    }

    // Endpoint de chat
    [HttpPost("{collectionId}/ask")]
    public async Task<IActionResult> AskQuestion(int collectionId, [FromBody] QueryRequest request)
    {
        var userId = GetUserId();
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
        
        if (collection == null) return NotFound("Coleção não encontrada.");

        // O seu QueryService já espera o systemContext, então está correto
        var response = await _queryService.AnswerQueryAsync(
            collection.Id, 
            collection.SystemContext,
            request.Question);
            
        return Ok(response);
    }
}