using Finovatech.Contratos;
using Finovatech.ProcessadorPagamento.Saga;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Finovatech.ProcessadorPagamento.Testes.Saga;

public class PagamentoStateMachineTestes : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        _serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<PagamentoStateMachine, PagamentoEstado>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(validateScopes: true);

        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task QuandoPagamentoIniciado_DeveMoverParaEstadoEmAnalise()
    {
        Guid correlacaoId = Guid.NewGuid();

        PagamentoIniciado evento = new()
        {
            PagamentoId       = Guid.NewGuid(),
            Valor             = 500m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "parceiro-a",
            ParceiroDestinoId = "parceiro-b",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(evento);

        ISagaStateMachineTestHarness<PagamentoStateMachine, PagamentoEstado> sagaHarness =
            _harness.GetSagaStateMachineHarness<PagamentoStateMachine, PagamentoEstado>();

        bool consumiu = await sagaHarness.Consumed.Any<PagamentoIniciado>();
        Assert.True(consumiu, "A saga deve ter consumido o evento PagamentoIniciado.");

        bool publicou = await _harness.Published.Any<PagamentoEnviadoParaAnalise>();
        Assert.True(publicou, "A saga deve ter publicado PagamentoEnviadoParaAnalise.");

        Guid? sagaId = await sagaHarness.Exists(correlacaoId, m => m.EmAnalise);
        Assert.NotNull(sagaId);
        Assert.Equal(correlacaoId, sagaId.Value);
    }

    [Fact]
    public async Task QuandoPagamentoEnviadoParaAnalise_DevePublicarCorrelacaoIdOriginal()
    {
        Guid correlacaoId = Guid.NewGuid();

        PagamentoIniciado evento = new()
        {
            PagamentoId       = Guid.NewGuid(),
            Valor             = 1000m,
            MoedaOrigem       = "EUR",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(evento);

        bool publicou = await _harness.Published.Any<PagamentoEnviadoParaAnalise>(
            msg => msg.Context.Message.CorrelacaoId == correlacaoId);

        Assert.True(publicou, "PagamentoEnviadoParaAnalise deve preservar o CorrelacaoId original.");
    }

    [Fact]
    public async Task QuandoAnaliseFraudeConcluidaAprovada_DeveMoverParaEstadoAprovado()
    {
        Guid correlacaoId = Guid.NewGuid();
        Guid pagamentoId  = Guid.NewGuid();

        PagamentoIniciado pagamentoIniciado = new()
        {
            PagamentoId       = pagamentoId,
            Valor             = 200m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(pagamentoIniciado);

        ISagaStateMachineTestHarness<PagamentoStateMachine, PagamentoEstado> sagaHarness =
            _harness.GetSagaStateMachineHarness<PagamentoStateMachine, PagamentoEstado>();

        // Aguarda saga entrar em EmAnalise antes de publicar a resposta
        Guid? emAnaliseSagaId = await sagaHarness.Exists(correlacaoId, m => m.EmAnalise);
        Assert.NotNull(emAnaliseSagaId);

        AnaliseFraudeConcluida analise = new()
        {
            PagamentoId       = pagamentoId,
            Aprovado          = true,
            Valor             = 200m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(analise);

        Guid? aprovadoSagaId = await sagaHarness.Exists(correlacaoId, m => m.Aprovado);
        Assert.NotNull(aprovadoSagaId);

        bool publicouAprovado = await _harness.Published.Any<PagamentoAprovado>(
            msg => msg.Context.Message.PagamentoId == pagamentoId);
        Assert.True(publicouAprovado, "A saga deve ter publicado PagamentoAprovado.");
    }

    [Fact]
    public async Task QuandoAnaliseFraudeConcluidaRejeitada_DeveMoverParaEstadoRejeitado()
    {
        Guid correlacaoId = Guid.NewGuid();
        Guid pagamentoId  = Guid.NewGuid();

        PagamentoIniciado pagamentoIniciado = new()
        {
            PagamentoId       = pagamentoId,
            Valor             = 9999m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(pagamentoIniciado);

        ISagaStateMachineTestHarness<PagamentoStateMachine, PagamentoEstado> sagaHarness =
            _harness.GetSagaStateMachineHarness<PagamentoStateMachine, PagamentoEstado>();

        Guid? emAnaliseSagaId = await sagaHarness.Exists(correlacaoId, m => m.EmAnalise);
        Assert.NotNull(emAnaliseSagaId);

        AnaliseFraudeConcluida analise = new()
        {
            PagamentoId       = pagamentoId,
            Aprovado          = false,
            MotivosRejeicao   = "Valor suspeito",
            Valor             = 9999m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(analise);

        Guid? rejeitadoSagaId = await sagaHarness.Exists(correlacaoId, m => m.Rejeitado);
        Assert.NotNull(rejeitadoSagaId);

        bool publicouRejeitado = await _harness.Published.Any<PagamentoRejeitado>(
            msg => msg.Context.Message.PagamentoId == pagamentoId &&
                   msg.Context.Message.Motivo == "Valor suspeito");
        Assert.True(publicouRejeitado, "A saga deve ter publicado PagamentoRejeitado com o motivo correto.");
    }

    [Fact]
    public async Task QuandoRejeitadaSemMotivo_DeveUsarMotivoDefault()
    {
        Guid correlacaoId = Guid.NewGuid();
        Guid pagamentoId  = Guid.NewGuid();

        PagamentoIniciado pagamentoIniciado = new()
        {
            PagamentoId       = pagamentoId,
            Valor             = 100m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(pagamentoIniciado);

        ISagaStateMachineTestHarness<PagamentoStateMachine, PagamentoEstado> sagaHarness =
            _harness.GetSagaStateMachineHarness<PagamentoStateMachine, PagamentoEstado>();

        Guid? emAnaliseSagaId = await sagaHarness.Exists(correlacaoId, m => m.EmAnalise);
        Assert.NotNull(emAnaliseSagaId);

        AnaliseFraudeConcluida analise = new()
        {
            PagamentoId       = pagamentoId,
            Aprovado          = false,
            MotivosRejeicao   = null,
            Valor             = 100m,
            MoedaOrigem       = "USD",
            MoedaDestino      = "BRL",
            ParceiroOrigemId  = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId      = correlacaoId,
        };

        await _harness.Bus.Publish(analise);

        bool publicouRejeitadoComDefault = await _harness.Published.Any<PagamentoRejeitado>(
            msg => msg.Context.Message.PagamentoId == pagamentoId &&
                   msg.Context.Message.Motivo == "Fraude detectada");
        Assert.True(publicouRejeitadoComDefault, "A saga deve usar 'Fraude detectada' como motivo default.");
    }
}
