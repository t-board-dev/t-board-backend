using System.Collections.Generic;
using System.Security.Claims;

namespace t_board.Services.Contracts
{
    public interface IJwtService
    {
        string GenerateToken(IEnumerable<Claim> claims);
        List<Claim> GetTokenClaims(string token);
        bool IsTokenValid(string token);
    }
}
