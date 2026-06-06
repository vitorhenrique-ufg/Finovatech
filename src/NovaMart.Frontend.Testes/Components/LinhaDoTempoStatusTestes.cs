using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaMart.Frontend.Components;
using NovaMart.Frontend.Modelos;
using NovaMart.Frontend.Servicos;

namespace NovaMart.Frontend.Testes.Components;

internal sealed class FakeServicoHubPedido : ServicoHubPedido
{
    public Func<AtualizacaoStatusViewModel, Task>? CallbackCapturado { get; private set; }
    public int TotalChamadasConectar { get; private set; }
    public Guid UltimoPedidoIdConectado { get; private set; }

    public FakeServicoHubPedido() : base(new ConfigurationBuilder().Build()) { }

    public override Task ConectaAoPedidoAsync(
        Guid pedidoId,
        Func<AtualizacaoStatusViewModel, Task> onStatus,
        string? jwtToken = null)
    {
        UltimoPedidoIdConectado = pedidoId;
        CallbackCapturado = onStatus;
        TotalChamadasConectar++;
        return Task.CompletedTask;
    }

    public override Task DesconecteDoPedidoAsync() => Task.CompletedTask;
}

internal sealed class FakeAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
}

public class LinhaDoTempoStatusTestes : TestContext
{
    private readonly FakeServicoHubPedido _hubPedido;

    public LinhaDoTempoStatusTestes()
    {
        _hubPedido = new FakeServicoHubPedido();
        Services.AddSingleton<ServicoHubPedido>(_hubPedido);
        Services.AddSingleton<AuthenticationStateProvider, FakeAuthStateProvider>();
        Services.AddSingleton<IHttpClientFactory>(new FakeHttpClientFactory());
    }

    [Fact]
    public void Renderiza_QuatroNosInicialmente()
    {
        Guid pedidoId = Guid.NewGuid();
        IRenderedComponent<LinhaDoTempoStatus> cut =
            RenderComponent<LinhaDoTempoStatus>(
                p => p.Add(c => c.PedidoId, pedidoId));

        System.Collections.Generic.IReadOnlyList<AngleSharp.Dom.IElement> nos =
            cut.FindAll(".flex.gap-4");
        Assert.True(nos.Count >= 4);
    }

    [Fact]
    public async Task QuandoAprovado_MostrarIconesDeConcluido()
    {
        Guid pedidoId = Guid.NewGuid();
        IRenderedComponent<LinhaDoTempoStatus> cut =
            RenderComponent<LinhaDoTempoStatus>(
                p => p.Add(c => c.PedidoId, pedidoId));

        Assert.NotNull(_hubPedido.CallbackCapturado);

        AtualizacaoStatusViewModel status = new(
            PagamentoId: pedidoId,
            Situacao: "Aprovado",
            Motivo: null,
            OcorridoEm: DateTimeOffset.UtcNow);

        await cut.InvokeAsync(() => _hubPedido.CallbackCapturado!(status));

        string markup = cut.Markup;
        Assert.Contains("fa-check", markup);
        Assert.Contains("Aprovado", markup);
    }

    [Fact]
    public async Task QuandoRejeitado_MostrarBotaoTentarNovamente()
    {
        Guid pedidoId = Guid.NewGuid();
        IRenderedComponent<LinhaDoTempoStatus> cut =
            RenderComponent<LinhaDoTempoStatus>(
                p => p.Add(c => c.PedidoId, pedidoId));

        Assert.NotNull(_hubPedido.CallbackCapturado);

        AtualizacaoStatusViewModel status = new(
            PagamentoId: pedidoId,
            Situacao: "Rejeitado",
            Motivo: "Valor acima do limite",
            OcorridoEm: DateTimeOffset.UtcNow);

        await cut.InvokeAsync(() => _hubPedido.CallbackCapturado!(status));

        string markup = cut.Markup;
        Assert.Contains("Tentar novamente", markup);
        Assert.Contains("Rejeitado", markup);
    }

    [Fact]
    public void ConectaAoHubNoMount()
    {
        Guid pedidoId = Guid.NewGuid();
        IRenderedComponent<LinhaDoTempoStatus> cut =
            RenderComponent<LinhaDoTempoStatus>(
                p => p.Add(c => c.PedidoId, pedidoId));

        Assert.Equal(1, _hubPedido.TotalChamadasConectar);
        Assert.Equal(pedidoId, _hubPedido.UltimoPedidoIdConectado);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
