using Finovatech.Contratos;
using Finovatech.GatewayPagamento.Dominio.ObjetosDeValor;

namespace Finovatech.GatewayPagamento.Dominio.Entidades;

public class Pagamento
{
    public Guid Id { get; private set; }
    public string ChaveIdempotencia { get; private set; } = string.Empty;
    public string MoedaOrigem { get; private set; } = string.Empty;
    public string MoedaDestino { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public string ParceiroOrigemId { get; private set; } = string.Empty;
    public string ParceiroDestinoId { get; private set; } = string.Empty;
    public SituacaoPagamento Situacao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public string? UsuarioId { get; private set; }

    private Pagamento() { }

    public void AtualizeSituacao(SituacaoPagamento novaSituacao)
    {
        Situacao = novaSituacao;
    }

    public static Pagamento Crie(
        ChaveIdempotencia chave,
        string moedaOrigem,
        string moedaDestino,
        decimal valor,
        string parceiroOrigemId,
        string parceiroDestinoId,
        string? usuarioId = null)
    {
        return new Pagamento
        {
            Id = Guid.NewGuid(),
            ChaveIdempotencia = chave.Valor,
            MoedaOrigem = moedaOrigem,
            MoedaDestino = moedaDestino,
            Valor = valor,
            ParceiroOrigemId = parceiroOrigemId,
            ParceiroDestinoId = parceiroDestinoId,
            Situacao = SituacaoPagamento.Pendente,
            CriadoEm = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId
        };
    }
}
