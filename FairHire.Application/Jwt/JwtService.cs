using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FairHire.API.Jwt; 

public class JwtService
{
    private readonly IConfiguration configuration;
    public  string IssueToken(Guid userId, string email)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = jwt.Key;

        if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Missing Jwt:Key");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        var jwtToken = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwtToken));
    }
}
