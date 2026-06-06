using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using NSubstitute;

namespace Finovatech.GatewayPagamento.Testes.ConsultePagamento;

public class ConsultePagamentoHandlerTestes
{
    private readonly IRepositorioPagamento _repositorio = Substitute.For<IRepositorioPagamento>();

    [Fact]
    public async Task ExecuteAsync_ComIdInexistente_DeveRetornarNull()
    {
        _repositorio.ConsultePorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Finovatech.GatewayPagamento.Dominio.Entidades.Pagamento?)null);

        var handler = new ConsultePagamentoHandler(_repositorio);
        var resultado = await handler.ExecuteAsync(new ConsultePagamentoConsulta(Guid.NewGuid()));

        Assert.Null(resultado);
    }
}
