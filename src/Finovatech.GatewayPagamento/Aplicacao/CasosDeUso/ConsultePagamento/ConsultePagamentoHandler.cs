using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;

namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;

public class ConsultePagamentoHandler(IRepositorioPagamento repositorio) : IConsultePagamentoHandler
{
    public async Task<ConsultePagamentoResultado?> ExecuteAsync(
        ConsultePagamentoConsulta consulta,
        CancellationToken ct = default)
    {
        Pagamento? pagamento = await repositorio.ConsultePorIdAsync(consulta.PagamentoId, ct);
        if (pagamento is null)
        {
            return null;
        }

        return new ConsultePagamentoResultado(
            pagamento.Id,
            pagamento.MoedaOrigem,
            pagamento.MoedaDestino,
            pagamento.Valor,
            pagamento.Situacao.ToString(),
            pagamento.CriadoEm);
    }
}
