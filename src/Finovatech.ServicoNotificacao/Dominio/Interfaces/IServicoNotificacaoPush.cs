namespace Finovatech.ServicoNotificacao.Dominio.Interfaces;

public interface IServicoNotificacaoPush
{
    Task EnvieAsync(
        string usuarioId,
        string titulo,
        string corpo,
        string url,
        CancellationToken ct = default);
}
