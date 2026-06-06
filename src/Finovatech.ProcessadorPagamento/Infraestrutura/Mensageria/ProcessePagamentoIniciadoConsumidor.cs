using Finovatech.Contratos;
using Finovatech.ProcessadorPagamento.Aplicacao.ProcessePagamentoIniciado;
using MassTransit;

namespace Finovatech.ProcessadorPagamento.Infraestrutura.Mensageria;

public class ProcessePagamentoIniciadoConsumidor(
    IProcessePagamentoIniciadoHandler handler,
    ILogger<ProcessePagamentoIniciadoConsumidor> logger)
    : IConsumer<PagamentoIniciado>
{
    public async Task Consume(ConsumeContext<PagamentoIniciado> context)
    {
        logger.LogInformation(
            "Mensagem recebida: PagamentoIniciado {PagamentoId}",
            context.Message.PagamentoId);

        await handler.ExecuteAsync(context.Message, context.CancellationToken);
    }
}
