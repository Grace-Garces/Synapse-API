using BrainAPI.Models.Auth;
using BrainAPI.Services; // Adicione este
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService; // 1. Injete o serviço do token

    public AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService) // 2. Adicione no construtor
    {
        _userManager = userManager;
        _tokenService = tokenService; // 3. Atribua
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        return Ok(new { Message = "Usuário criado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { Message = "Email ou senha inválidos." });
        }
        
        // --- 4. GERE E RETORNE O TOKEN ---
        var token = _tokenService.GenerateToken(user);
        return Ok(new { Token = token, Email = user.Email });
    }
}