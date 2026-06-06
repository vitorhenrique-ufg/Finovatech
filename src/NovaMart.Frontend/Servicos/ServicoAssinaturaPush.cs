using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NovaMart.Frontend.Modelos;
using System.Net.Http.Headers;

namespace NovaMart.Frontend.Servicos;

public class ServicoAssinaturaPush(
    IJSRuntime js,
    IHttpClientFactory httpFactory,
    IConfiguration config,
    AuthenticationStateProvider authState) : IServicoAssinaturaPush
{
    private readonly string _vapidPublicKey =
        config["Push:PublicKey"] ?? string.Empty;

    public bool PushSuportado => true;

    public async Task<bool> ConsulteAssinaturaAtivadaAsync()
    {
        try
        {
            bool suportado = await js.InvokeAsync<bool>("PushInterop.isSupported");
            if (!suportado)
            {
                return false;
            }

            string estado = await js.InvokeAsync<string>("PushInterop.getPermissionState");
            if (estado != "granted")
            {
                return false;
            }
            object? sub = await js.InvokeAsync<object?>("PushInterop.getCurrentSubscription");
            return sub is not null;
        }
        catch { return false; }
    }

    public async Task<bool> AtiveAsync()
    {
        try
        {
            bool suportado = await js.InvokeAsync<bool>("PushInterop.isSupported");
            if (!suportado || string.IsNullOrWhiteSpace(_vapidPublicKey))
            {
                return false;
            }

            SubscriptionDto? subscription = await js.InvokeAsync<SubscriptionDto?>(
                "PushInterop.subscribe", _vapidPublicKey);
            if (subscription is null)
            {
                return false;
            }

            HttpClient client = await CrieClienteAutenticadoAsync();
            HttpResponseMessage resp = await client.PostAsJsonAsync(
                "/notifications/subscribe", subscription);

            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task DesativeAsync()
    {
        try
        {
            await js.InvokeVoidAsync("PushInterop.unsubscribe");
            HttpClient client = await CrieClienteAutenticadoAsync();
            await client.DeleteAsync("/notifications/subscribe");
        }
        catch { /* ignore */ }
    }

    public async Task<bool> VerifiquePedidoPermissaoMostradoAsync()
    {
        try { return await js.InvokeAsync<bool>("PushInterop.jaPerguntouPermissao"); }
        catch { return true; }
    }

    public async Task MarquePedidoPermissaoMostradoAsync()
    {
        try { await js.InvokeVoidAsync("PushInterop.marcarPermissaoPergunada"); }
        catch { /* ignore */ }
    }

    public async Task<List<NotificacaoItem>> ConsulteNotificacoesAsync()
    {
        try
        {
            List<NotificacaoItem>? lista = await js.InvokeAsync<List<NotificacaoItem>?>(
                "PushInterop.getNotificacoes");
            return lista ?? [];
        }
        catch { return []; }
    }

    public async Task<int> ContarNaoLidasAsync()
    {
        try { return await js.InvokeAsync<int>("PushInterop.contarNaoLidas"); }
        catch { return 0; }
    }

    public async Task MarqueTodasLidasAsync()
    {
        try { await js.InvokeVoidAsync("PushInterop.marqueTodasLidas"); }
        catch { /* ignore */ }
    }

    private async Task<HttpClient> CrieClienteAutenticadoAsync()
    {
        HttpClient client = httpFactory.CreateClient("gateway");
        AuthenticationState state = await authState.GetAuthenticationStateAsync();
        string? token = state.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    private record SubscriptionDto(string Endpoint, string P256dh, string Auth);
}
