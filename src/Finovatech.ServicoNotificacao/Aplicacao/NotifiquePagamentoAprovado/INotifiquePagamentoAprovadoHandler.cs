using Finovatech.Contratos;

namespace Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoAprovado;

public interface INotifiquePagamentoAprovadoHandler
{
    Task NotifiqueAsync(PagamentoAprovado evento, CancellationToken ct = default);
}
