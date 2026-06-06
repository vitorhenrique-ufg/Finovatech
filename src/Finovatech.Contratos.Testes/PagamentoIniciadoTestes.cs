using Finovatech.Contratos;

namespace Finovatech.Contratos.Testes;

public class PagamentoIniciadoTestes
{
    [Fact]
    public void CriarPagamentoIniciado_DevePreencherCorrelacaoIdAutomaticamente()
    {
        var evento = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            Valor = 1000m,
            ParceiroOrigemId = "parceiro-1",
            ParceiroDestinoId = "parceiro-2"
        };

        Assert.NotEqual(Guid.Empty, evento.CorrelacaoId);
    }

    [Fact]
    public void CriarPagamentoIniciado_DevePreencherOcorridoEmAutomaticamente()
    {
        var antes = DateTimeOffset.UtcNow;

        var evento = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            Valor = 1000m,
            ParceiroOrigemId = "parceiro-1",
            ParceiroDestinoId = "parceiro-2"
        };

        Assert.True(evento.OcorridoEm >= antes);
    }

    [Fact]
    public void DoisPagamentosIniciados_DevemTerCorrelacaoIdsDistintos()
    {
        var evento1 = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            Valor = 100m,
            ParceiroOrigemId = "parceiro-1",
            ParceiroDestinoId = "parceiro-2"
        };

        var evento2 = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "EUR",
            MoedaDestino = "BRL",
            Valor = 200m,
            ParceiroOrigemId = "parceiro-3",
            ParceiroDestinoId = "parceiro-4"
        };

        Assert.NotEqual(evento1.CorrelacaoId, evento2.CorrelacaoId);
    }
}
