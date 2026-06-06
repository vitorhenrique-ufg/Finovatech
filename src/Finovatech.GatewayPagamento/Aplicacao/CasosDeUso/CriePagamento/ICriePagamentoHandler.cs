namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;

public interface ICriePagamentoHandler
{
    Task<CriePagamentoResultado> ExecuteAsync(CriePagamentoComando comando, CancellationToken ct = default);
}
