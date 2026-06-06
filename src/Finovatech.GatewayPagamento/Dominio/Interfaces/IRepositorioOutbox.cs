using Finovatech.GatewayPagamento.Dominio.Entidades;

namespace Finovatech.GatewayPagamento.Dominio.Interfaces;

public interface IRepositorioOutbox
{
    Task AdicioneAsync(RegistroOutbox registro, CancellationToken ct = default);
    Task<IReadOnlyList<RegistroOutbox>> ConsultePendentesAsync(int limite = 50, CancellationToken ct = default);
    Task SalveAlteracoesAsync(CancellationToken ct = default);
}
