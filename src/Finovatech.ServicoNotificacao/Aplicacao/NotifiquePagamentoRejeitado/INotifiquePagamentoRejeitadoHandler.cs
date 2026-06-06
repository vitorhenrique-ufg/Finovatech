using Finovatech.Contratos;

namespace Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoRejeitado;

public interface INotifiquePagamentoRejeitadoHandler
{
    Task NotifiqueAsync(PagamentoRejeitado evento, CancellationToken ct = default);
}
