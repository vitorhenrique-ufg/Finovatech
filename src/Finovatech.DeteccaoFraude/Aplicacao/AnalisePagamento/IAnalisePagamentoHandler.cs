using Finovatech.Contratos;

namespace Finovatech.DeteccaoFraude.Aplicacao.AnalisePagamento;

public interface IAnalisePagamentoHandler
{
    Task ExecuteAsync(PagamentoEnviadoParaAnalise evento, CancellationToken ct = default);
}
