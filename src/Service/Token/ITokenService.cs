using GateEntryExit.Domain;
using System.Security.Claims;

namespace GateEntryExit.Service.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
