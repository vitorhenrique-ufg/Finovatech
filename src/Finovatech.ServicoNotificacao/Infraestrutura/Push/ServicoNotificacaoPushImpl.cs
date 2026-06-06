using System.Net;
using System.Text.Json;
using Finovatech.ServicoNotificacao.Dominio.Entidades;
using Finovatech.ServicoNotificacao.Dominio.Interfaces;
using WebPush;

namespace Finovatech.ServicoNotificacao.Infraestrutura.Push;

public class ServicoNotificacaoPushImpl(
    IRepositorioAssinaturaPush repositorio,
    IConfiguration config,
    ILogger<ServicoNotificacaoPushImpl> logger) : IServicoNotificacaoPush
{
    private readonly WebPushClient _cliente = new();

    public async Task EnvieAsync(
        string usuarioId,
        string titulo,
        string corpo,
        string url,
        CancellationToken ct = default)
    {
        AssinaturaPush? assinatura = await repositorio.ConsultePorUsuarioAsync(usuarioId, ct);
        if (assinatura is null)
        {
            logger.LogDebug("Sem assinatura push para usuário {UsuarioId}", usuarioId);
            return;
        }

        string? publicKey = config["Push:PublicKey"];
        string? privateKey = config["Push:PrivateKey"];
        string subject = config["Push:Subject"] ?? "mailto:dev@finovatech.com";

        if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey))
        {
            logger.LogWarning("Chaves VAPID não configuradas — push desabilitado");
            return;
        }

        VapidDetails vapid = new(subject, publicKey, privateKey);
        PushSubscription subscription = new(assinatura.Endpoint, assinatura.P256dh, assinatura.Auth);

        string payload = JsonSerializer.Serialize(new { titulo, corpo, url });

        try
        {
            await _cliente.SendNotificationAsync(subscription, payload, vapid);
            logger.LogInformation("Push enviado para {UsuarioId}: {Titulo}", usuarioId, titulo);
        }
        catch (WebPushException ex) when
            (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Subscription expirada para {UsuarioId} — removendo", usuarioId);
            await repositorio.RemoveAsync(usuarioId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar push para {UsuarioId}", usuarioId);
        }
    }
}
