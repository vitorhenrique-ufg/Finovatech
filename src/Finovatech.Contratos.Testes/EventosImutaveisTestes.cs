using Finovatech.Contratos;

namespace Finovatech.Contratos.Testes;

public class EventosImutaveisTestes
{
    [Fact]
    public void EventoWith_DeveCriarNovaInstanciaSemAlterarOriginal()
    {
        var original = new PagamentoIniciado
        {
            PagamentoId = Guid.NewGuid(),
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            Valor = 500m,
            ParceiroOrigemId = "p1",
            ParceiroDestinoId = "p2"
        };

        var modificado = original with { Valor = 999m };

        Assert.Equal(500m, original.Valor);
        Assert.Equal(999m, modificado.Valor);
        Assert.Equal(original.CorrelacaoId, modificado.CorrelacaoId);
    }

    [Fact]
    public void SituacaoPagamento_DeveConterTodosOsEstados()
    {
        var estados = Enum.GetValues<SituacaoPagamento>();

        Assert.Contains(SituacaoPagamento.Pendente, estados);
        Assert.Contains(SituacaoPagamento.EmAnalise, estados);
        Assert.Contains(SituacaoPagamento.Aprovado, estados);
        Assert.Contains(SituacaoPagamento.Rejeitado, estados);
        Assert.Contains(SituacaoPagamento.Notificado, estados);
    }
}
