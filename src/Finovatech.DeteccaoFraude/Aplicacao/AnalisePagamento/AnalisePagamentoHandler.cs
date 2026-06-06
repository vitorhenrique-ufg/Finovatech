using System.Threading.RateLimiting;
using Finovatech.Contratos;
using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;
using MassTransit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Finovatech.DeteccaoFraude.Aplicacao.AnalisePagamento;

public class AnalisePagamentoHandler(
    IServicoAnaliseFraude servicoRegras,
    IServicoAnaliseFraudeIA servicoIA,
    IPublishEndpoint publishEndpoint,
    ILogger<AnalisePagamentoHandler> logger) : IAnalisePagamentoHandler
{
    private static readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 5,
        })
        .AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(15) })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
        })
        .Build();

    public async Task ExecuteAsync(PagamentoEnviadoParaAnalise evento, CancellationToken ct = default)
    {
        System.Diagnostics.Activity.Current?.SetTag("pagamento.correlacaoId", evento.CorrelacaoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.id", evento.PagamentoId.ToString());
        System.Diagnostics.Activity.Current?.SetTag("pagamento.valor", evento.Valor.ToString("F2"));
        System.Diagnostics.Activity.Current?.SetTag("pagamento.moedaOrigem", evento.MoedaOrigem);

        logger.LogInformation(
            "Analisando pagamento {PagamentoId} valor={Valor} {MoedaOrigem}→{MoedaDestino}",
            evento.PagamentoId, evento.Valor, evento.MoedaOrigem, evento.MoedaDestino);

        // 1. Análise por regras de negócio (rápida, determinística)
        ResultadoAnalise resultadoRegras = servicoRegras.Analise(evento.Valor, evento.MoedaOrigem);

        // 2. Análise por IA — só executa se houver provedor configurado
        ResultadoAnalise resultadoFinal;
        if (!servicoIA.IAHabilitada)
        {
            logger.LogDebug(
                "IA não configurada para {PagamentoId} — usando somente regras de negócio",
                evento.PagamentoId);
            resultadoFinal = resultadoRegras;
        }
        else
        {
            ResultadoAnalise? resultadoIA = await AnaliseComTimeoutAsync(evento, ct);
            resultadoFinal = EscolhaResultadoFinal(resultadoRegras, resultadoIA, evento.PagamentoId);
        }

        System.Diagnostics.Activity.Current?.SetTag("analise.aprovado", resultadoFinal.Aprovado.ToString());
        System.Diagnostics.Activity.Current?.SetTag("analise.analisadoIA", resultadoFinal.AnalisadoPorIA.ToString());
        if (resultadoFinal.NivelRisco.HasValue)
        {
            System.Diagnostics.Activity.Current?.SetTag("analise.nivelRisco", resultadoFinal.NivelRisco.Value.ToString("F2"));
        }

        if (resultadoFinal.Motivo is not null)
        {
            System.Diagnostics.Activity.Current?.SetTag("analise.motivo", resultadoFinal.Motivo);
        }

        AnaliseFraudeConcluida analise = new()
        {
            PagamentoId = evento.PagamentoId,
            Aprovado = resultadoFinal.Aprovado,
            MotivosRejeicao = resultadoFinal.Motivo,
            Valor = evento.Valor,
            MoedaOrigem = evento.MoedaOrigem,
            MoedaDestino = evento.MoedaDestino,
            ParceiroOrigemId = evento.ParceiroOrigemId,
            ParceiroDestinoId = evento.ParceiroDestinoId,
            UsuarioId = evento.UsuarioId,
            CorrelacaoId = evento.CorrelacaoId
        };

        await _pipeline.ExecuteAsync(async token =>
            await publishEndpoint.Publish(analise, token), ct);

        logger.LogInformation(
            "AnaliseFraudeConcluida para {PagamentoId}: Aprovado={Aprovado}, IA={AnalisadoIA}",
            evento.PagamentoId, resultadoFinal.Aprovado, resultadoFinal.AnalisadoPorIA);
    }

    private async Task<ResultadoAnalise?> AnaliseComTimeoutAsync(
        PagamentoEnviadoParaAnalise evento,
        CancellationToken ctConsumidor)
    {
        using CancellationTokenSource iaCts =
            CancellationTokenSource.CreateLinkedTokenSource(ctConsumidor);
        iaCts.CancelAfter(TimeSpan.FromSeconds(8));

        try
        {
            return await servicoIA.AnaliseAsync(
                evento.Valor,
                evento.MoedaOrigem,
                evento.MoedaDestino,
                evento.ParceiroOrigemId,
                evento.ParceiroDestinoId,
                evento.CorrelacaoId,
                iaCts.Token);
        }
        catch (OperationCanceledException) when (!ctConsumidor.IsCancellationRequested)
        {
            logger.LogWarning(
                "Timeout de 8 s na análise de IA para {PagamentoId} — usando somente regras",
                evento.PagamentoId);
            return null;
        }
    }

    private ResultadoAnalise EscolhaResultadoFinal(
        ResultadoAnalise resultadoRegras,
        ResultadoAnalise? resultadoIA,
        Guid pagamentoId)
    {
        if (resultadoIA is null)
        {
            logger.LogWarning(
                "IA indisponível para {PagamentoId} — usando somente regras de negócio",
                pagamentoId);
            return resultadoRegras;
        }

        if (!resultadoRegras.Aprovado)
        {
            string motivoCombinado = resultadoIA.AnalisadoPorIA && !string.IsNullOrEmpty(resultadoIA.ResumoIA)
                ? $"{resultadoRegras.Motivo} | IA: {resultadoIA.ResumoIA}"
                : resultadoRegras.Motivo ?? "Rejeitado por regras de negócio";

            return ResultadoAnalise.Rejeitar(
                motivoCombinado,
                resultadoIA.ResumoIA ?? string.Empty,
                resultadoIA.NivelRisco ?? 1.0m);
        }

        if (!resultadoIA.Aprovado)
        {
            logger.LogWarning(
                "IA detectou fraude em {PagamentoId} mesmo aprovado pelas regras: {Motivo}",
                pagamentoId, resultadoIA.Motivo);

            return ResultadoAnalise.Rejeitar(
                resultadoIA.Motivo ?? "Rejeitado por análise de IA",
                resultadoIA.ResumoIA ?? string.Empty,
                resultadoIA.NivelRisco ?? 1.0m);
        }

        return ResultadoAnalise.Aprovar(
            resultadoIA.ResumoIA ?? string.Empty,
            resultadoIA.NivelRisco ?? 0.0m);
    }
}
