using System.Threading.RateLimiting;
using Finovatech.Contratos;
using MassTransit;
using Polly;
using Polly.Retry;

namespace Finovatech.ProcessadorPagamento.Aplicacao.ProcessePagamentoIniciado;

public class ProcessePagamentoIniciadoHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<ProcessePagamentoIniciadoHandler> logger) : IProcessePagamentoIniciadoHandler
{
    private static readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = 20,
            QueueLimit = 10,
        })
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        })
        .Build();

    public async Task ExecuteAsync(PagamentoIniciado evento, CancellationToken ct = default)
    {
        System.Diagnostics.Activity.Current?.SetTag("pagamento.correlacaoId", evento.CorrelacaoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.id", evento.PagamentoId.ToString());

        logger.LogInformation(
            "Encaminhando pagamento {PagamentoId} para análise de fraude (CorrelacaoId={CorrelacaoId})",
            evento.PagamentoId, evento.CorrelacaoId);

        PagamentoEnviadoParaAnalise envio = new()
        {
            PagamentoId = evento.PagamentoId,
            Valor = evento.Valor,
            MoedaOrigem = evento.MoedaOrigem,
            MoedaDestino = evento.MoedaDestino,
            ParceiroOrigemId = evento.ParceiroOrigemId,
            ParceiroDestinoId = evento.ParceiroDestinoId,
            UsuarioId = evento.UsuarioId,
            CorrelacaoId = evento.CorrelacaoId
        };

        await _pipeline.ExecuteAsync(async token =>
            await publishEndpoint.Publish(envio, token), ct);

        logger.LogInformation(
            "PagamentoEnviadoParaAnalise publicado para {PagamentoId}",
            evento.PagamentoId);
    }
}
