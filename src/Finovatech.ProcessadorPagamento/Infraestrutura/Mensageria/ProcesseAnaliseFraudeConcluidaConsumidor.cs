using Finovatech.Contratos;
using Finovatech.ProcessadorPagamento.Aplicacao.ProcesseAnaliseFraudeConcluida;
using MassTransit;

namespace Finovatech.ProcessadorPagamento.Infraestrutura.Mensageria;

public class ProcesseAnaliseFraudeConcluidaConsumidor(
    IProcesseAnaliseFraudeConcluidaHandler handler,
    ILogger<ProcesseAnaliseFraudeConcluidaConsumidor> logger)
    : IConsumer<AnaliseFraudeConcluida>
{
    public async Task Consume(ConsumeContext<AnaliseFraudeConcluida> context)
    {
        logger.LogInformation(
            "Mensagem recebida: AnaliseFraudeConcluida {PagamentoId} Aprovado={Aprovado}",
            context.Message.PagamentoId, context.Message.Aprovado);

        await handler.ExecuteAsync(context.Message, context.CancellationToken);
    }
}
