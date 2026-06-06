using System.Threading.RateLimiting;
using Finovatech.Contratos;
using MassTransit;
using Polly;
using Polly.Retry;

namespace Finovatech.ProcessadorPagamento.Aplicacao.ProcesseAnaliseFraudeConcluida;

public class ProcesseAnaliseFraudeConcluidaHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<ProcesseAnaliseFraudeConcluidaHandler> logger) : IProcesseAnaliseFraudeConcluidaHandler
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

    public async Task ExecuteAsync(AnaliseFraudeConcluida evento, CancellationToken ct = default)
    {
        System.Diagnostics.Activity.Current?.SetTag("pagamento.correlacaoId", evento.CorrelacaoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.id", evento.PagamentoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.aprovado", evento.Aprovado.ToString());

        if (evento.Aprovado)
        {
            PagamentoAprovado aprovado = new()
            {
                PagamentoId = evento.PagamentoId,
                Valor = evento.Valor,
                MoedaOrigem = evento.MoedaOrigem,
                MoedaDestino = evento.MoedaDestino,
                ParceiroDestinoId = evento.ParceiroDestinoId,
                UsuarioId = evento.UsuarioId,
                CorrelacaoId = evento.CorrelacaoId
            };

            await _pipeline.ExecuteAsync(async token =>
                await publishEndpoint.Publish(aprovado, token), ct);

            logger.LogInformation(
                "PagamentoAprovado publicado para {PagamentoId} (CorrelacaoId={CorrelacaoId})",
                evento.PagamentoId, evento.CorrelacaoId);
        }
        else
        {
            PagamentoRejeitado rejeitado = new()
            {
                PagamentoId = evento.PagamentoId,
                Motivo = evento.MotivosRejeicao ?? "Fraude detectada",
                ParceiroOrigemId = evento.ParceiroOrigemId,
                UsuarioId = evento.UsuarioId,
                CorrelacaoId = evento.CorrelacaoId
            };

            await _pipeline.ExecuteAsync(async token =>
                await publishEndpoint.Publish(rejeitado, token), ct);

            logger.LogInformation(
                "PagamentoRejeitado publicado para {PagamentoId}: {Motivo}",
                evento.PagamentoId, rejeitado.Motivo);
        }
    }
}
