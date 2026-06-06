using Finovatech.Contratos;
using Finovatech.ProcessadorPagamento.Aplicacao.ProcesseAnaliseFraudeConcluida;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Finovatech.ProcessadorPagamento.Testes.ProcesseAnaliseFraudeConcluida;

public class ProcesseAnaliseFraudeConcluidaHandlerTestes
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger<ProcesseAnaliseFraudeConcluidaHandler> _logger =
        Substitute.For<ILogger<ProcesseAnaliseFraudeConcluidaHandler>>();

    private ProcesseAnaliseFraudeConcluidaHandler CrieHandler() =>
        new(_publishEndpoint, _logger);

    private static AnaliseFraudeConcluida CrieEvento(bool aprovado, string? motivo = null) =>
        new()
        {
            PagamentoId = Guid.NewGuid(),
            Aprovado = aprovado,
            MotivosRejeicao = motivo,
            Valor = 1000m,
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            ParceiroOrigemId = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId = Guid.NewGuid()
        };

    [Fact]
    public async Task ExecuteAsync_QuandoAprovado_DevePublicarPagamentoAprovado()
    {
        var evento = CrieEvento(aprovado: true);
        var handler = CrieHandler();

        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<PagamentoAprovado>(e => e.PagamentoId == evento.PagamentoId),
            Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive()
            .Publish(Arg.Any<PagamentoRejeitado>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoRejeitado_DevePublicarPagamentoRejeitadoComMotivo()
    {
        var evento = CrieEvento(aprovado: false, motivo: "Valor acima do limite");
        var handler = CrieHandler();

        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<PagamentoRejeitado>(e =>
                e.PagamentoId == evento.PagamentoId &&
                e.Motivo == "Valor acima do limite"),
            Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive()
            .Publish(Arg.Any<PagamentoAprovado>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoRejeitadoSemMotivo_DeveUsarMotivoDefault()
    {
        var evento = CrieEvento(aprovado: false, motivo: null);
        var handler = CrieHandler();

        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<PagamentoRejeitado>(e => e.Motivo == "Fraude detectada"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoAprovado_DevePreservarCorrelacaoId()
    {
        var correlacaoId = Guid.NewGuid();
        var evento = CrieEvento(aprovado: true) with { CorrelacaoId = correlacaoId };

        PagamentoAprovado? capturado = null;
        _publishEndpoint
            .When(e => e.Publish(Arg.Any<PagamentoAprovado>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturado = ci.Arg<PagamentoAprovado>());

        var handler = CrieHandler();
        await handler.ExecuteAsync(evento);

        Assert.NotNull(capturado);
        Assert.Equal(correlacaoId, capturado.CorrelacaoId);
    }
}
