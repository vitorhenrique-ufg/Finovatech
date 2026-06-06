using Microsoft.AspNetCore.SignalR.Client;
using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public class ServicoHubPedido(IConfiguration configuration) : IServicoHubPedido
{
    private HubConnection? _conexao;
    private Guid _pedidoId;
    private readonly string _hubUrl =
        (configuration["GatewayApi:BaseUrl"] ?? "http://localhost:8000") + "/hubs/payments";

    public event Func<string, Task>? OnReconectando;
    public event Func<Task>? OnReconectado;
    public event Func<string, Task>? OnConexaoEncerrada;

    public virtual async Task ConectaAoPedidoAsync(
        Guid pedidoId,
        Func<AtualizacaoStatusViewModel, Task> onStatus,
        string? jwtToken = null)
    {
        _pedidoId = pedidoId;

        _conexao = new HubConnectionBuilder()
            .WithUrl(_hubUrl, opts =>
            {
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    opts.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
                }
            })
            .WithAutomaticReconnect(new PoliticaReconexao())
            .Build();

        _conexao.On("AtualizacaoStatus", onStatus);

        _conexao.Reconnecting += async erro =>
        {
            if (OnReconectando is not null)
            {
                await OnReconectando(erro?.Message ?? "Reconectando...");
            }
        };

        _conexao.Reconnected += async _ =>
        {
            await _conexao.InvokeAsync("AssineStatusPagamento", _pedidoId.ToString());
            if (OnReconectado is not null)
            {
                await OnReconectado();
            }
        };

        _conexao.Closed += async erro =>
        {
            if (OnConexaoEncerrada is not null)
            {
                await OnConexaoEncerrada(erro?.Message ?? "Conexão encerrada.");
            }
        };

        await _conexao.StartAsync();
        await _conexao.InvokeAsync("AssineStatusPagamento", _pedidoId.ToString());
    }

    public virtual async Task DesconecteDoPedidoAsync()
    {
        if (_conexao is not null)
        {
            await _conexao.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexao is not null)
        {
            await _conexao.DisposeAsync();
        }
    }
}

file sealed class PoliticaReconexao : IRetryPolicy
{
    private static readonly TimeSpan[] _intervalos =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(60),
    ];

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        if (retryContext.PreviousRetryCount < _intervalos.Length)
        {
            return _intervalos[retryContext.PreviousRetryCount];
        }

        return TimeSpan.FromSeconds(60);
    }
}
