using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace NovaMart.Frontend.Autenticacao;

public class ProvedorEstadoAutenticacao(IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = httpContextAccessor.HttpContext?.Request.Cookies["jwt-token"];
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
        return Task.FromResult(new AuthenticationState(ParseJwt(token)));
    }

    public void NotifiqueAlteracaoEstado() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static ClaimsPrincipal ParseJwt(string token)
    {
        try
        {
            string[] partes = token.Split('.');
            if (partes.Length != 3)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            string payload = partes[1];
            int padding = payload.Length % 4;
            if (padding != 0)
            {
                payload += new string('=', 4 - padding);
            }

            payload = payload.Replace('-', '+').Replace('_', '/');
            Dictionary<string, object>? claims = JsonSerializer.Deserialize<Dictionary<string, object>>(Convert.FromBase64String(payload));
            if (claims is null)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
            return new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, claims.GetValueOrDefault("sub")?.ToString() ?? ""),
                new Claim(ClaimTypes.Email, claims.GetValueOrDefault("email")?.ToString() ?? ""),
                new Claim(ClaimTypes.Name, claims.GetValueOrDefault("email")?.ToString() ?? ""),
            ], "jwt"));
        }
        catch { return new ClaimsPrincipal(new ClaimsIdentity()); }
    }
}
