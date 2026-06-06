using Finovatech.ServicoNotificacao.Dominio.Entidades;

namespace Finovatech.ServicoNotificacao.Dominio.Interfaces;

public interface IRepositorioAssinaturaPush
{
    Task SalveAsync(AssinaturaPush assinatura, CancellationToken ct = default);
    Task<AssinaturaPush?> ConsultePorUsuarioAsync(string usuarioId, CancellationToken ct = default);
    Task RemoveAsync(string usuarioId, CancellationToken ct = default);
}
