using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BrainAPI.Services;

public class JwtService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(IdentityUser user)
    {
        // 1. Chave de segurança
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Claims (Informações do usuário no token)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // ID do usuário
            new Claim(JwtRegisteredClaimNames.Email, user.Email!) // Email
            // Você pode adicionar mais claims aqui se precisar
        };

        // 3. Criação do Token
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7), // Token expira em 7 dias
            signingCredentials: creds
        );

        // 4. Escreve o token como string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}