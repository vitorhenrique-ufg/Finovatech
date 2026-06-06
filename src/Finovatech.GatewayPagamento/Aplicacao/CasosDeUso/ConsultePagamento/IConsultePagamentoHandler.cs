namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;

public interface IConsultePagamentoHandler
{
    Task<ConsultePagamentoResultado?> ExecuteAsync(ConsultePagamentoConsulta consulta, CancellationToken ct = default);
}
