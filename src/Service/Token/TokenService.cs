using GateEntryExit.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GateEntryExit.Service.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(UserManager<AppUser> userManager,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public string GenerateAccessToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII
            .GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);

            var roles = _userManager.GetRolesAsync(user).Result;

            List<Claim> claims =
            [
                new (JwtRegisteredClaimNames.Email,user.Email??""),
                new (JwtRegisteredClaimNames.Name,user.FullName??""),
                new (JwtRegisteredClaimNames.NameId,user.Id ??""),
                new (JwtRegisteredClaimNames.Aud,
                _configuration.GetSection("JWTSetting").GetSection("validAudience").Value!),
                new (JwtRegisteredClaimNames.Iss,
                _configuration.GetSection("JWTSetting").GetSection("validIssuer").Value!)
            ];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            });

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // https://github.com/dejanmauer/AuthenticateAspNetCoreApiWithJWTandRefreshTokens/blob/master/README.md
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSetting").GetSection("securityKey").Value!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
