using Finovatech.Contratos;
using Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoRejeitado;
using Finovatech.ServicoNotificacao.Dominio;
using Finovatech.ServicoNotificacao.Infraestrutura.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Finovatech.ServicoNotificacao.Testes.NotifiquePagamentoRejeitado;

public class NotifiquePagamentoRejeitadoHandlerTestes
{
    private readonly IHubContext<StatusPagamentoHub> _hubContext = Substitute.For<IHubContext<StatusPagamentoHub>>();
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly ILogger<NotifiquePagamentoRejeitadoHandler> _logger = Substitute.For<ILogger<NotifiquePagamentoRejeitadoHandler>>();

    public NotifiquePagamentoRejeitadoHandlerTestes()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);
    }

    private NotifiquePagamentoRejeitadoHandler CrieHandler() => new(_hubContext, _logger);

    private static PagamentoRejeitado CrieEvento(Guid? id = null, string motivo = "Fraude") => new()
    {
        PagamentoId = id ?? Guid.NewGuid(), Motivo = motivo, ParceiroOrigemId = "p1", CorrelacaoId = Guid.NewGuid()
    };

    [Fact]
    public async Task NotifiqueAsync_DeveChamarGroupComPagamentoIdCorreto()
    {
        Guid id = Guid.NewGuid();
        await CrieHandler().NotifiqueAsync(CrieEvento(id));
        _hubClients.Received(1).Group(id.ToString());
    }

    [Fact]
    public async Task NotifiqueAsync_DeveEnviarMensagemAtualizacaoStatus()
    {
        await CrieHandler().NotifiqueAsync(CrieEvento());
        await _clientProxy.Received(1).SendCoreAsync("AtualizacaoStatus",
            Arg.Is<object[]>(a => a.Length == 1 && a[0] is AtualizacaoStatusPagamento),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifiqueAsync_DeveEnviarSituacaoRejeitado()
    {
        AtualizacaoStatusPagamento? cap = null;
        await _clientProxy.SendCoreAsync("AtualizacaoStatus",
            Arg.Do<object[]>(a => cap = a[0] as AtualizacaoStatusPagamento), Arg.Any<CancellationToken>());
        await CrieHandler().NotifiqueAsync(CrieEvento());
        Assert.NotNull(cap);
        Assert.Equal("Rejeitado", cap.Situacao);
    }

    [Fact]
    public async Task NotifiqueAsync_DeveEnviarMotivoDoEvento()
    {
        AtualizacaoStatusPagamento? cap = null;
        await _clientProxy.SendCoreAsync("AtualizacaoStatus",
            Arg.Do<object[]>(a => cap = a[0] as AtualizacaoStatusPagamento), Arg.Any<CancellationToken>());
        await CrieHandler().NotifiqueAsync(CrieEvento(motivo: "Limite excedido"));
        Assert.NotNull(cap);
        Assert.Equal("Limite excedido", cap.Motivo);
    }

    [Fact]
    public async Task NotifiqueAsync_DeveEnviarPagamentoIdNoPayload()
    {
        Guid id = Guid.NewGuid();
        AtualizacaoStatusPagamento? cap = null;
        await _clientProxy.SendCoreAsync("AtualizacaoStatus",
            Arg.Do<object[]>(a => cap = a[0] as AtualizacaoStatusPagamento), Arg.Any<CancellationToken>());
        await CrieHandler().NotifiqueAsync(CrieEvento(id));
        Assert.NotNull(cap);
        Assert.Equal(id, cap.PagamentoId);
    }

    [Fact]
    public async Task NotifiqueAsync_DeveUsarOcorridoEmDoEvento()
    {
        DateTimeOffset ts = new(2026, 5, 30, 15, 30, 0, TimeSpan.Zero);
        AtualizacaoStatusPagamento? cap = null;
        await _clientProxy.SendCoreAsync("AtualizacaoStatus",
            Arg.Do<object[]>(a => cap = a[0] as AtualizacaoStatusPagamento), Arg.Any<CancellationToken>());
        await CrieHandler().NotifiqueAsync(CrieEvento() with { OcorridoEm = ts });
        Assert.NotNull(cap);
        Assert.Equal(ts, cap.OcorridoEm);
    }
}
