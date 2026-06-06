using Finovatech.Contratos;

namespace Finovatech.Contratos.Testes;

public class AnaliseFraudeConcluidaTestes
{
    [Fact]
    public void CriarAnaliseFraudeConcluida_Aprovada_DeveManterCorrelacaoIdOriginal()
    {
        var correlacaoId = Guid.NewGuid();

        var evento = new AnaliseFraudeConcluida
        {
            PagamentoId = Guid.NewGuid(),
            Aprovado = true,
            MotivosRejeicao = null,
            Valor = 1000m,
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            ParceiroOrigemId = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId = correlacaoId
        };

        Assert.Equal(correlacaoId, evento.CorrelacaoId);
        Assert.True(evento.Aprovado);
        Assert.Null(evento.MotivosRejeicao);
    }

    [Fact]
    public void CriarAnaliseFraudeConcluida_Rejeitada_DeveConterMotivoRejeicao()
    {
        var evento = new AnaliseFraudeConcluida
        {
            PagamentoId = Guid.NewGuid(),
            Aprovado = false,
            MotivosRejeicao = "Padrão de transação suspeito",
            Valor = 99000m,
            MoedaOrigem = "USD",
            MoedaDestino = "BRL",
            ParceiroOrigemId = "p1",
            ParceiroDestinoId = "p2",
            CorrelacaoId = Guid.NewGuid()
        };

        Assert.False(evento.Aprovado);
        Assert.NotNull(evento.MotivosRejeicao);
    }
}
