using Finovatech.Contratos;
using MassTransit;

namespace Finovatech.ProcessadorPagamento.Saga;

public class PagamentoStateMachine : MassTransitStateMachine<PagamentoEstado>
{
    private readonly ILogger<PagamentoStateMachine> _logger;

    public State EmAnalise { get; private set; } = null!;
    public State Aprovado { get; private set; } = null!;
    public State Rejeitado { get; private set; } = null!;

    public Event<PagamentoIniciado> PagamentoIniciado { get; private set; } = null!;
    public Event<AnaliseFraudeConcluida> AnaliseFraudeConcluida { get; private set; } = null!;

    public PagamentoStateMachine(ILogger<PagamentoStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Event(() => PagamentoIniciado, x =>
            x.CorrelateById(ctx => ctx.Message.CorrelacaoId));

        Event(() => AnaliseFraudeConcluida, x =>
            x.CorrelateById(ctx => ctx.Message.CorrelacaoId));

        Initially(
            When(PagamentoIniciado)
                .Then(ctx =>
                {
                    ctx.Saga.PagamentoId = ctx.Message.PagamentoId;
                    ctx.Saga.Valor = ctx.Message.Valor;
                    ctx.Saga.MoedaOrigem = ctx.Message.MoedaOrigem;
                    ctx.Saga.MoedaDestino = ctx.Message.MoedaDestino;
                    ctx.Saga.ParceiroOrigemId = ctx.Message.ParceiroOrigemId;
                    ctx.Saga.ParceiroDestinoId = ctx.Message.ParceiroDestinoId;
                    ctx.Saga.IniciadoEm = ctx.Message.OcorridoEm;

                    _logger.LogInformation(
                        "[SAGA] PagamentoIniciado recebido: {PagamentoId} (CorrelacaoId={CorrelacaoId})",
                        ctx.Message.PagamentoId, ctx.Message.CorrelacaoId);
                })
                .PublishAsync(ctx => ctx.Init<PagamentoEnviadoParaAnalise>(new PagamentoEnviadoParaAnalise
                {
                    PagamentoId = ctx.Saga.PagamentoId,
                    Valor = ctx.Saga.Valor,
                    MoedaOrigem = ctx.Saga.MoedaOrigem,
                    MoedaDestino = ctx.Saga.MoedaDestino,
                    ParceiroOrigemId = ctx.Saga.ParceiroOrigemId,
                    ParceiroDestinoId = ctx.Saga.ParceiroDestinoId,
                    CorrelacaoId = ctx.Saga.CorrelationId,
                }))
                .TransitionTo(EmAnalise));

        During(EmAnalise,
            When(AnaliseFraudeConcluida, ctx => ctx.Message.Aprovado)
                .Then(ctx =>
                {
                    _logger.LogInformation(
                        "[SAGA] Análise aprovada para {PagamentoId}",
                        ctx.Saga.PagamentoId);
                })
                .PublishAsync(ctx => ctx.Init<PagamentoAprovado>(new PagamentoAprovado
                {
                    PagamentoId = ctx.Saga.PagamentoId,
                    Valor = ctx.Saga.Valor,
                    MoedaOrigem = ctx.Saga.MoedaOrigem,
                    MoedaDestino = ctx.Saga.MoedaDestino,
                    ParceiroDestinoId = ctx.Saga.ParceiroDestinoId,
                    CorrelacaoId = ctx.Saga.CorrelationId,
                }))
                .TransitionTo(Aprovado),

            When(AnaliseFraudeConcluida, ctx => !ctx.Message.Aprovado)
                .Then(ctx =>
                {
                    _logger.LogInformation(
                        "[SAGA] Análise rejeitada para {PagamentoId}: {Motivo}",
                        ctx.Saga.PagamentoId, ctx.Message.MotivosRejeicao);
                })
                .PublishAsync(ctx => ctx.Init<PagamentoRejeitado>(new PagamentoRejeitado
                {
                    PagamentoId = ctx.Saga.PagamentoId,
                    Motivo = ctx.Message.MotivosRejeicao ?? "Fraude detectada",
                    ParceiroOrigemId = ctx.Saga.ParceiroOrigemId,
                    CorrelacaoId = ctx.Saga.CorrelationId,
                }))
                .TransitionTo(Rejeitado));

        SetCompletedWhenFinalized();
    }
}
