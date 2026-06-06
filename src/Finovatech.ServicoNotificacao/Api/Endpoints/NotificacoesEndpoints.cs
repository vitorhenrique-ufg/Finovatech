using System.Text.Json;
using Finovatech.ServicoNotificacao.Api.Modelos;
using Finovatech.ServicoNotificacao.Dominio.Entidades;
using Finovatech.ServicoNotificacao.Dominio.Interfaces;

namespace Finovatech.ServicoNotificacao.Api.Endpoints;

public static class NotificacoesEndpoints
{
    public static void MapNotificacoesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications/vapid-public-key", (IConfiguration config) =>
            Results.Ok(new { publicKey = config["Push:PublicKey"] ?? string.Empty })
        ).AllowAnonymous();

        app.MapPost("/notifications/subscribe", async (
            AssineRequest request,
            HttpContext ctx,
            IRepositorioAssinaturaPush repositorio,
            CancellationToken ct) =>
        {
            string? usuarioId = ExtraiUsuarioId(ctx);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Results.Unauthorized();
            }

            AssinaturaPush assinatura = new(usuarioId, request.Endpoint, request.P256dh, request.Auth);
            await repositorio.SalveAsync(assinatura, ct);
            return Results.Ok();
        });

        app.MapDelete("/notifications/subscribe", async (
            HttpContext ctx,
            IRepositorioAssinaturaPush repositorio,
            CancellationToken ct) =>
        {
            string? usuarioId = ExtraiUsuarioId(ctx);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Results.Unauthorized();
            }

            await repositorio.RemoveAsync(usuarioId, ct);
            return Results.Ok();
        });
    }

    private static string? ExtraiUsuarioId(HttpContext ctx)
    {
        string? auth = ctx.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string token = auth["Bearer ".Length..].Trim();
        string[] partes = token.Split('.');
        if (partes.Length != 3)
        {
            return null;
        }

        try
        {
            string payload = partes[1];
            int padding = payload.Length % 4;
            if (padding != 0)
            {
                payload += new string('=', 4 - padding);
            }
            payload = payload.Replace('-', '+').Replace('_', '/');
            using JsonDocument doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            return doc.RootElement.TryGetProperty("sub", out JsonElement sub) ? sub.GetString() : null;
        }
        catch { return null; }
    }
}
