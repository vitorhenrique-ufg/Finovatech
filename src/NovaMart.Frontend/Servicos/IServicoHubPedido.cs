using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public interface IServicoHubPedido : IAsyncDisposable
{
    event Func<string, Task>? OnReconectando;
    event Func<Task>? OnReconectado;
    event Func<string, Task>? OnConexaoEncerrada;

    Task ConectaAoPedidoAsync(
        Guid pedidoId,
        Func<AtualizacaoStatusViewModel, Task> onStatus,
        string? jwtToken = null);

    Task DesconecteDoPedidoAsync();
}
