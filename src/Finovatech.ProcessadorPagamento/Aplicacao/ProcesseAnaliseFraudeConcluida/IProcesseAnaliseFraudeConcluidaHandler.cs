using Finovatech.Contratos;

namespace Finovatech.ProcessadorPagamento.Aplicacao.ProcesseAnaliseFraudeConcluida;

public interface IProcesseAnaliseFraudeConcluidaHandler
{
    Task ExecuteAsync(AnaliseFraudeConcluida evento, CancellationToken ct = default);
}
