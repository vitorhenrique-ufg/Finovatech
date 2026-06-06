using Finovatech.Contratos;
using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;
using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using NSubstitute;

namespace Finovatech.GatewayPagamento.Testes.CriePagamento;

public class CriePagamentoHandlerTestes
{
    private readonly IRepositorioPagamento _repositorioPagamento = Substitute.For<IRepositorioPagamento>();
    private readonly IRepositorioOutbox _repositorioOutbox = Substitute.For<IRepositorioOutbox>();
    private readonly IServicoIdempotencia _servicoIdempotencia = Substitute.For<IServicoIdempotencia>();

    private CriePagamentoHandler CrieHandler() =>
        new(_repositorioPagamento, _repositorioOutbox, _servicoIdempotencia);

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_DevePersistirPagamentoERegistroOutbox()
    {
        var comando = new CriePagamentoComando(
            ChaveIdempotencia: Guid.NewGuid().ToString(),
            MoedaOrigem: "USD",
            MoedaDestino: "BRL",
            Valor: 1000m,
            ParceiroOrigemId: "parceiro-1",
            ParceiroDestinoId: "parceiro-2");

        _servicoIdempotencia.ConsulteRespostaAsync(comando.ChaveIdempotencia, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var handler = CrieHandler();
        var resultado = await handler.ExecuteAsync(comando);

        Assert.NotEqual(Guid.Empty, resultado.PagamentoId);
        Assert.Equal(SituacaoPagamento.Pendente, resultado.Situacao);
        await _repositorioPagamento.Received(1).AdicioneAsync(Arg.Any<Pagamento>(), Arg.Any<CancellationToken>());
        await _repositorioOutbox.Received(1).AdicioneAsync(Arg.Any<RegistroOutbox>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComChaveDuplicada_DeveRetornarRespostaCacheada()
    {
        var chave = Guid.NewGuid().ToString();
        var pagamentoIdCacheado = Guid.NewGuid();
        var respostaCacheada = System.Text.Json.JsonSerializer.Serialize(
            new CriePagamentoResultado(pagamentoIdCacheado, SituacaoPagamento.Pendente));

        _servicoIdempotencia.ConsulteRespostaAsync(chave, Arg.Any<CancellationToken>())
            .Returns(respostaCacheada);

        var comando = new CriePagamentoComando(
            ChaveIdempotencia: chave,
            MoedaOrigem: "USD",
            MoedaDestino: "BRL",
            Valor: 500m,
            ParceiroOrigemId: "p1",
            ParceiroDestinoId: "p2");

        var handler = CrieHandler();
        var resultado = await handler.ExecuteAsync(comando);

        Assert.True(resultado.FoiCacheado);
        await _repositorioPagamento.DidNotReceive().AdicioneAsync(Arg.Any<Pagamento>(), Arg.Any<CancellationToken>());
    }
}
