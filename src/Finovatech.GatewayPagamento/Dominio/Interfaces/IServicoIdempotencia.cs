namespace Finovatech.GatewayPagamento.Dominio.Interfaces;

public interface IServicoIdempotencia
{
    Task<string?> ConsulteRespostaAsync(string chave, CancellationToken ct = default);
    Task RegistreRespostaAsync(string chave, string resposta, TimeSpan ttl, CancellationToken ct = default);
}
