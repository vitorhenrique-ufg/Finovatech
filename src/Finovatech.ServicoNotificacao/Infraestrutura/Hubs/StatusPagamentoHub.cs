using Microsoft.AspNetCore.SignalR;

namespace Finovatech.ServicoNotificacao.Infraestrutura.Hubs;

public sealed class StatusPagamentoHub : Hub
{
    public async Task AssineStatusPagamento(string pagamentoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pagamentoId);
    }
}
