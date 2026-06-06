using Finovatech.GatewayPagamento.Dominio.ObjetosDeValor;

namespace Finovatech.GatewayPagamento.Testes.Dominio;

public class ChaveIdempotenciaTestes
{
    [Fact]
    public void CriarChave_ComGuidValido_DeveSerValida()
    {
        var chave = new ChaveIdempotencia(Guid.NewGuid().ToString());
        Assert.True(chave.EhValida);
    }

    [Fact]
    public void CriarChave_ComStringVazia_DeveSerInvalida()
    {
        var chave = new ChaveIdempotencia(string.Empty);
        Assert.False(chave.EhValida);
    }

    [Fact]
    public void DuasChavesIguais_DevemSerIguaisComEqualityOperator()
    {
        var valor = Guid.NewGuid().ToString();
        var chave1 = new ChaveIdempotencia(valor);
        var chave2 = new ChaveIdempotencia(valor);
        Assert.Equal(chave1, chave2);
    }
}
