using Finovatech.Contratos;
using Finovatech.DeteccaoFraude.Aplicacao.AnalisePagamento;
using MassTransit;

namespace Finovatech.DeteccaoFraude.Infraestrutura.Mensageria;

public class AnalisePagamentoConsumidor(
    IAnalisePagamentoHandler handler,
    ILogger<AnalisePagamentoConsumidor> logger)
    : IConsumer<PagamentoEnviadoParaAnalise>
{
    public async Task Consume(ConsumeContext<PagamentoEnviadoParaAnalise> context)
    {
        logger.LogInformation(
            "Mensagem recebida: PagamentoEnviadoParaAnalise {PagamentoId}",
            context.Message.PagamentoId);

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await handler.ExecuteAsync(context.Message, cts.Token);
    }
}
