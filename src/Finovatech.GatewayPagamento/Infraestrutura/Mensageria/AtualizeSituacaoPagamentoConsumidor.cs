using Finovatech.Contratos;
using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using MassTransit;

namespace Finovatech.GatewayPagamento.Infraestrutura.Mensageria;

public class PagamentoAprovadoConsumidor(
    IRepositorioPagamento repositorio,
    ILogger<PagamentoAprovadoConsumidor> logger)
    : IConsumer<PagamentoAprovado>
{
    public async Task Consume(ConsumeContext<PagamentoAprovado> context)
    {
        Pagamento? pagamento = await repositorio.ConsultePorIdRastreaveAsync(
            context.Message.PagamentoId, context.CancellationToken);

        if (pagamento is null)
        {
            logger.LogWarning("Pagamento {Id} não encontrado para atualizar situação.", context.Message.PagamentoId);
            return;
        }

        pagamento.AtualizeSituacao(SituacaoPagamento.Aprovado);
        await repositorio.SalveAlteracoesAsync(context.CancellationToken);

        logger.LogInformation("Pagamento {Id} atualizado para Aprovado.", pagamento.Id);
    }
}

public class PagamentoRejeitadoConsumidor(
    IRepositorioPagamento repositorio,
    ILogger<PagamentoRejeitadoConsumidor> logger)
    : IConsumer<PagamentoRejeitado>
{
    public async Task Consume(ConsumeContext<PagamentoRejeitado> context)
    {
        Pagamento? pagamento = await repositorio.ConsultePorIdRastreaveAsync(
            context.Message.PagamentoId, context.CancellationToken);

        if (pagamento is null)
        {
            logger.LogWarning("Pagamento {Id} não encontrado para atualizar situação.", context.Message.PagamentoId);
            return;
        }

        pagamento.AtualizeSituacao(SituacaoPagamento.Rejeitado);
        await repositorio.SalveAlteracoesAsync(context.CancellationToken);

        logger.LogInformation("Pagamento {Id} atualizado para Rejeitado.", pagamento.Id);
    }
}
