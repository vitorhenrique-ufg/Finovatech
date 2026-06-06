using Finovatech.Contratos;
using Finovatech.DeteccaoFraude.Aplicacao.AnalisePagamento;
using Finovatech.DeteccaoFraude.Dominio.Servicos;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Finovatech.DeteccaoFraude.Testes.AnalisePagamento;

public class AnalisePagamentoHandlerTestes
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger<AnalisePagamentoHandler> _logger =
        Substitute.For<ILogger<AnalisePagamentoHandler>>();

    private static PagamentoEnviadoParaAnalise CrieEvento(decimal valor = 1000m, string moeda = "USD") =>
        new()
        {
            PagamentoId = Guid.NewGuid(),
            Valor = valor,
            MoedaOrigem = moeda,
            MoedaDestino = "BRL",
            ParceiroOrigemId = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId = Guid.NewGuid()
        };

    private AnalisePagamentoHandler CrieHandlerComRegras(
        decimal valorMaximo = 50_000m,
        string[]? moedasBloqueadas = null)
    {
        var config = new ConfiguracaoAnaliseFraude
        {
            ValorMaximoPorTransacao = valorMaximo,
            MoedasBloqueadas = moedasBloqueadas ?? []
        };
        var servico = new Finovatech.DeteccaoFraude.Dominio.Servicos.ServicoAnaliseFraude(Options.Create(config));
        return new AnalisePagamentoHandler(servico, _publishEndpoint, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_ComPagamentoNormal_DevePublicarAnaliseConcluIdaAprovada()
    {
        var evento = CrieEvento(valor: 1_000m);
        var handler = CrieHandlerComRegras(valorMaximo: 50_000m);

        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<AnaliseFraudeConcluida>(e =>
                e.PagamentoId == evento.PagamentoId &&
                e.Aprovado == true &&
                e.CorrelacaoId == evento.CorrelacaoId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComValorAcimaDoLimite_DevePublicarAnaliseConcluIdaRejeitada()
    {
        var evento = CrieEvento(valor: 99_000m);
        var handler = CrieHandlerComRegras(valorMaximo: 50_000m);

        await handler.ExecuteAsync(evento);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<AnaliseFraudeConcluida>(e =>
                e.PagamentoId == evento.PagamentoId &&
                e.Aprovado == false &&
                e.MotivosRejeicao != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DevePassarContextoCompleto()
    {
        var evento = CrieEvento();
        var handler = CrieHandlerComRegras();

        AnaliseFraudeConcluida? capturado = null;
        _publishEndpoint
            .When(e => e.Publish(Arg.Any<AnaliseFraudeConcluida>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturado = ci.Arg<AnaliseFraudeConcluida>());

        await handler.ExecuteAsync(evento);

        Assert.NotNull(capturado);
        Assert.Equal(evento.Valor, capturado.Valor);
        Assert.Equal(evento.MoedaOrigem, capturado.MoedaOrigem);
        Assert.Equal(evento.MoedaDestino, capturado.MoedaDestino);
        Assert.Equal(evento.ParceiroOrigemId, capturado.ParceiroOrigemId);
        Assert.Equal(evento.ParceiroDestinoId, capturado.ParceiroDestinoId);
    }
}
