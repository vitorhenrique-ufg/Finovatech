namespace Finovatech.GatewayPagamento.Dominio.Excecoes;

public class PagamentoDuplicadoExcecao : Exception
{
    public PagamentoDuplicadoExcecao(string chave)
        : base($"Pagamento com chave de idempotência '{chave}' já foi processado.") { }
}
