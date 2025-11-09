using Microsoft.AspNetCore.Identity;

namespace BrainAPI.Services;

public interface ITokenService
{
    string GenerateToken(IdentityUser user);
}