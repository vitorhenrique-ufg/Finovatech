using Finovatech.DeteccaoFraude.Dominio.Servicos;
using Microsoft.Extensions.Options;

namespace Finovatech.DeteccaoFraude.Testes.ServicoAnaliseFraude;

public class ServicoAnaliseFraudeTestes
{
    private static Finovatech.DeteccaoFraude.Dominio.Servicos.ServicoAnaliseFraude CrieServico(
        decimal valorMaximo = 50_000m,
        string[]? moedasBloqueadas = null)
    {
        var config = new ConfiguracaoAnaliseFraude
        {
            ValorMaximoPorTransacao = valorMaximo,
            MoedasBloqueadas = moedasBloqueadas ?? []
        };
        return new Finovatech.DeteccaoFraude.Dominio.Servicos.ServicoAnaliseFraude(
            Options.Create(config));
    }

    [Fact]
    public void Analise_ComValorAbaixoDoLimite_DeveAprovar()
    {
        var servico = CrieServico(valorMaximo: 50_000m);
        var resultado = servico.Analise(1_000m, "USD");
        Assert.True(resultado.Aprovado);
        Assert.Null(resultado.Motivo);
    }

    [Fact]
    public void Analise_ComValorAcimaDoLimite_DeveRejeitar()
    {
        var servico = CrieServico(valorMaximo: 50_000m);
        var resultado = servico.Analise(51_000m, "USD");
        Assert.False(resultado.Aprovado);
        Assert.NotNull(resultado.Motivo);
    }

    [Fact]
    public void Analise_ComMoedaBloqueada_DeveRejeitar()
    {
        var servico = CrieServico(moedasBloqueadas: ["XBT", "ZAR"]);
        var resultado = servico.Analise(100m, "XBT");
        Assert.False(resultado.Aprovado);
        Assert.Contains("XBT", resultado.Motivo ?? "");
    }

    [Fact]
    public void Analise_ComMoedaPermitida_DeveAprovar()
    {
        var servico = CrieServico(moedasBloqueadas: ["XBT"]);
        var resultado = servico.Analise(100m, "USD");
        Assert.True(resultado.Aprovado);
    }

    [Theory]
    [InlineData(50_000.00, true)]
    [InlineData(50_000.01, false)]
    public void Analise_NoLimiteDoValor_DeveAplicarRegra(decimal valor, bool aprovado)
    {
        var servico = CrieServico(valorMaximo: 50_000m);
        var resultado = servico.Analise(valor, "USD");
        Assert.Equal(aprovado, resultado.Aprovado);
    }
}
