using System.Threading.RateLimiting;
using Finovatech.Contratos;
using Finovatech.ServicoNotificacao.Dominio;
using Finovatech.ServicoNotificacao.Dominio.Interfaces;
using Finovatech.ServicoNotificacao.Infraestrutura.Hubs;
using Microsoft.AspNetCore.SignalR;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoAprovado;

public class NotifiquePagamentoAprovadoHandler(
    IHubContext<StatusPagamentoHub> hubContext,
    IServicoNotificacaoPush servicoPush,
    ILogger<NotifiquePagamentoAprovadoHandler> logger) : INotifiquePagamentoAprovadoHandler
{
    private static readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = 15,
            QueueLimit = 5,
        })
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(60),
            MinimumThroughput = 3,
            BreakDuration = TimeSpan.FromSeconds(30),
        })
        .Build();

    public async Task NotifiqueAsync(PagamentoAprovado evento, CancellationToken ct = default)
    {
        System.Diagnostics.Activity.Current?.SetTag("pagamento.correlacaoId", evento.CorrelacaoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.id", evento.PagamentoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("notificacao.tipo", "Aprovado");

        logger.LogInformation(
            "Notificando aprovação do pagamento {PagamentoId} (CorrelacaoId={CorrelacaoId})",
            evento.PagamentoId, evento.CorrelacaoId);

        AtualizacaoStatusPagamento payload = new(
            PagamentoId: evento.PagamentoId,
            Situacao: "Aprovado",
            Motivo: null,
            OcorridoEm: evento.OcorridoEm);

        await _pipeline.ExecuteAsync(async token =>
            await hubContext.Clients
                .Group(evento.PagamentoId.ToString())
                .SendAsync("AtualizacaoStatus", payload, token), ct);

        if (!string.IsNullOrEmpty(evento.UsuarioId))
        {
            await servicoPush.EnvieAsync(
                evento.UsuarioId,
                "Pagamento Aprovado",
                $"Seu pedido foi aprovado! Valor: {evento.Valor:C2}",
                $"/order/{evento.PagamentoId}",
                ct);
        }
    }
}
