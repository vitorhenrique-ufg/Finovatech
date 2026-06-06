using Finovatech.GatewayPagamento.Api.Modelos;
using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;
using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;
using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;

namespace Finovatech.GatewayPagamento.Api.Endpoints;

public static class PagamentosEndpoints
{
    public static void MapPagamentosEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder grupo = app.MapGroup("/payments").WithTags("Payments");

        grupo.MapGet("", async (
            HttpContext ctx,
            IRepositorioPagamento repositorio,
            CancellationToken ct) =>
        {
            string? usuarioId = ExtraiUsuarioIdDoJwt(ctx);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Results.Unauthorized();
            }

            IReadOnlyList<Pagamento> pagamentos = await repositorio.ConsultePorUsuarioAsync(usuarioId, ct);

            return Results.Ok(pagamentos.Select(p =>
                new PagamentoListaItem(p.Id, p.Situacao.ToString(), p.Valor, p.CriadoEm)));
        });

        grupo.MapPost("/", async (
            CriarPagamentoRequest request,
            HttpContext ctx,
            ICriePagamentoHandler handler,
            CancellationToken ct) =>
        {
            string? chaveIdempotencia = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            {
                return Results.BadRequest(new { erro = "Header 'Idempotency-Key' é obrigatório." });
            }

            string? usuarioId = ExtraiUsuarioIdDoJwt(ctx);

            CriePagamentoComando comando = new(
                chaveIdempotencia,
                request.MoedaOrigem,
                request.MoedaDestino,
                request.Valor,
                request.ParceiroOrigemId,
                request.ParceiroDestinoId,
                usuarioId);

            CriePagamentoResultado resultado = await handler.ExecuteAsync(comando, ct);
            PagamentoResponse resposta = new(resultado.PagamentoId, resultado.Situacao.ToString(), resultado.FoiCacheado);

            return resultado.FoiCacheado
                ? Results.Ok(resposta)
                : Results.Created($"/payments/{resultado.PagamentoId}", resposta);
        });

        grupo.MapGet("/{id:guid}", async (
            Guid id,
            IConsultePagamentoHandler handler,
            CancellationToken ct) =>
        {
            ConsultePagamentoResultado? resultado = await handler.ExecuteAsync(new ConsultePagamentoConsulta(id), ct);
            if (resultado is null)
            {
                return Results.NotFound(new { erro = $"Pagamento {id} não encontrado." });
            }

            return Results.Ok(new PagamentoDetalheResponse(
                resultado.PagamentoId,
                resultado.MoedaOrigem,
                resultado.MoedaDestino,
                resultado.Valor,
                resultado.Situacao,
                resultado.CriadoEm));
        });
    }

    private static string? ExtraiUsuarioIdDoJwt(HttpContext ctx)
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
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(Convert.FromBase64String(payload));
            return doc.RootElement.TryGetProperty("sub", out System.Text.Json.JsonElement sub) ? sub.GetString() : null;
        }
        catch { return null; }
    }
}
