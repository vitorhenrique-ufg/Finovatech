using Finovatech.GatewayPagamento.Dominio.Entidades;

namespace Finovatech.GatewayPagamento.Dominio.Interfaces;

public interface IRepositorioPagamento
{
    Task<Pagamento?> ConsultePorIdAsync(Guid id, CancellationToken ct = default);
    Task<Pagamento?> ConsultePorIdRastreaveAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Pagamento>> ConsultePorUsuarioAsync(string usuarioId, CancellationToken ct = default);
    Task AdicioneAsync(Pagamento pagamento, CancellationToken ct = default);
    Task SalveAlteracoesAsync(CancellationToken ct = default);
}
