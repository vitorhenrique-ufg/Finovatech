using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Finovatech.GatewayApi.Autenticacao;

public class ServicoJwt(IConfiguration configuration) : IServicoJwt
{
    private readonly string _chave = configuration["Jwt:ChaveSecreta"]
        ?? throw new InvalidOperationException("Jwt:ChaveSecreta não configurada");
    private readonly string _emissor = configuration["Jwt:Emissor"] ?? "finovatech";
    private readonly string _audiencia = configuration["Jwt:Audiencia"] ?? "novamart";

    public string EmitaToken(string userId, string email)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_chave);
        SymmetricSecurityKey chaveSeguranca = new(keyBytes);
        SigningCredentials credenciais = new(chaveSeguranca, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub,   userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        ];

        JwtSecurityToken tokenJwt = new(
            issuer: _emissor,
            audience: _audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(tokenJwt);
    }

    public bool ValideToken(string token, out string? userId)
    {
        userId = null;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        try
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            byte[] keyBytes = Encoding.UTF8.GetBytes(_chave);
            TokenValidationParameters parametros = new()
            {
                ValidateIssuer = true,
                ValidIssuer = _emissor,
                ValidateAudience = true,
                ValidAudience = _audiencia,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);
            userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return true;
        }
        catch { return false; }
    }
}
