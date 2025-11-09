using BrainAPI.Models;
using BrainAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace BrainAPI.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PersonaController : ControllerBase
{
    private readonly PersonaService _personaService;

    public PersonaController(PersonaService personaService)
    {
        _personaService = personaService;
    }

    /// <summary>
    /// Gera um contexto de persona detalhado com base em uma breve descrição.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(PersonaResponse), 200)]
    public async Task<IActionResult> GenerateContext([FromBody] PersonaRequest request)
    {
        var context = await _personaService.GeneratePersonaContextAsync(request.Description);
        return Ok(new PersonaResponse { GeneratedContext = context });
    }
}