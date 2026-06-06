namespace Finovatech.GatewayPagamento.Dominio.ObjetosDeValor;

public sealed record ChaveIdempotencia(string Valor)
{
    public bool EhValida => !string.IsNullOrWhiteSpace(Valor);

    public override string ToString() => Valor;
}
