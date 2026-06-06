using Finovatech.Contratos;
using Finovatech.ProcessadorPagamento.Aplicacao.ProcessePagamentoIniciado;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Finovatech.ProcessadorPagamento.Testes.ProcessePagamentoIniciado;

public class ProcessePagamentoIniciadoHandlerTestes
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger<ProcessePagamentoIniciadoHandler> _logger =
        Substitute.For<ILogger<ProcessePagamentoIniciadoHandler>>();

    private ProcessePagamentoIniciadoHandler CrieHandler() =>
        new(_publishEndpoint, _logger);

    [Fact]
    public async Task ExecuteAsync_DevePublicarPagamentoEnviadoParaAnaliseComDadosCompletos()
    {
        var evento = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            Valor = 1000m,
            ParceiroOrigemId = "parceiro-1",
            ParceiroDestinoId = "parceiro-2",
            CorrelacaoId = Guid.NewGuid()
        };

        var handler = CrieHandler();
        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<PagamentoEnviadoParaAnalise>(e =>
                e.PagamentoId == evento.PagamentoId &&
                e.Valor == evento.Valor &&
                e.MoedaOrigem == evento.MoedaOrigem &&
                e.MoedaDestino == evento.MoedaDestino &&
                e.ParceiroOrigemId == evento.ParceiroOrigemId &&
                e.ParceiroDestinoId == evento.ParceiroDestinoId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DevePreservarCorrelacaoIdOriginal()
    {
        var correlacaoId = Guid.NewGuid();
        var evento = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "EUR",
            MoedaDestino = "BRL",
            Valor = 500m,
            ParceiroOrigemId = "p3",
            ParceiroDestinoId = "p4",
            CorrelacaoId = correlacaoId
        };

        PagamentoEnviadoParaAnalise? capturado = null;
        _publishEndpoint
            .When(e => e.Publish(Arg.Any<PagamentoEnviadoParaAnalise>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturado = ci.Arg<PagamentoEnviadoParaAnalise>());

        var handler = CrieHandler();
        await handler.ExecuteAsync(evento);

        Assert.NotNull(capturado);
        Assert.Equal(correlacaoId, capturado.CorrelacaoId);
    }
}
