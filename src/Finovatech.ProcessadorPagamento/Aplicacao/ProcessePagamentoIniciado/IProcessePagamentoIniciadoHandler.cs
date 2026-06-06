using Finovatech.Contratos;

namespace Finovatech.ProcessadorPagamento.Aplicacao.ProcessePagamentoIniciado;

public interface IProcessePagamentoIniciadoHandler
{
    Task ExecuteAsync(PagamentoIniciado evento, CancellationToken ct = default);
}
