using Finovatech.Contratos;
using Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoAprovado;
using MassTransit;

namespace Finovatech.ServicoNotificacao.Infraestrutura.Mensageria;

public class PagamentoAprovadoConsumidor(
    INotifiquePagamentoAprovadoHandler handler,
    ILogger<PagamentoAprovadoConsumidor> logger)
    : IConsumer<PagamentoAprovado>
{
    public async Task Consume(ConsumeContext<PagamentoAprovado> context)
    {
        logger.LogInformation("Mensagem recebida: PagamentoAprovado {PagamentoId}", context.Message.PagamentoId);
        await handler.NotifiqueAsync(context.Message, context.CancellationToken);
    }
}
