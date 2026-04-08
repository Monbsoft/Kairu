using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kairu.Application.Identity.Commands.GetOrCreateUser;
using Microsoft.IdentityModel.Tokens;

namespace Kairu.Api.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int ExpiryHours => _configuration.GetValue<int>("Jwt:ExpiryHours", 24);

    public string GenerateToken(GetOrCreateUserResult user)
    {
        var claims = new[]
        {
            new Claim("sub", user.UserId.Value.ToString()),
            new Claim("name", user.DisplayName),
            new Claim("login", user.Login),
        };

        return CreateToken(claims);
    }

    private string CreateToken(Claim[] claims)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(ExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
