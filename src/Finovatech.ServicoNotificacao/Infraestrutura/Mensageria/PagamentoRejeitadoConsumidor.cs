using Finovatech.Contratos;
using Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoRejeitado;
using MassTransit;

namespace Finovatech.ServicoNotificacao.Infraestrutura.Mensageria;

public class PagamentoRejeitadoConsumidor(
    INotifiquePagamentoRejeitadoHandler handler,
    ILogger<PagamentoRejeitadoConsumidor> logger)
    : IConsumer<PagamentoRejeitado>
{
    public async Task Consume(ConsumeContext<PagamentoRejeitado> context)
    {
        logger.LogInformation("Mensagem recebida: PagamentoRejeitado {PagamentoId}", context.Message.PagamentoId);
        await handler.NotifiqueAsync(context.Message, context.CancellationToken);
    }
}
