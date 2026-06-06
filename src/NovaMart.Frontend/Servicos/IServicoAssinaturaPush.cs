using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public interface IServicoAssinaturaPush
{
    bool PushSuportado { get; }

    Task<bool> ConsulteAssinaturaAtivadaAsync();
    Task<bool> AtiveAsync();
    Task DesativeAsync();

    Task<bool> VerifiquePedidoPermissaoMostradoAsync();
    Task MarquePedidoPermissaoMostradoAsync();

    Task<List<NotificacaoItem>> ConsulteNotificacoesAsync();
    Task<int> ContarNaoLidasAsync();
    Task MarqueTodasLidasAsync();
}
