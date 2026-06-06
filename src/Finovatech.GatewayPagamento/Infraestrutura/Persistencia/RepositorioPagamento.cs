using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finovatech.GatewayPagamento.Infraestrutura.Persistencia;

public class RepositorioPagamento(ContextoPagamento contexto) : IRepositorioPagamento
{
    public async Task<Pagamento?> ConsultePorIdAsync(Guid id, CancellationToken ct = default)
        => await contexto.Pagamentos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Pagamento>> ConsultePorUsuarioAsync(string usuarioId, CancellationToken ct = default)
        => await contexto.Pagamentos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync(ct);

    public async Task<Pagamento?> ConsultePorIdRastreaveAsync(Guid id, CancellationToken ct = default)
        => await contexto.Pagamentos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AdicioneAsync(Pagamento pagamento, CancellationToken ct = default)
    {
        await contexto.Pagamentos.AddAsync(pagamento, ct);
    }

    public async Task SalveAlteracoesAsync(CancellationToken ct = default)
        => await contexto.SaveChangesAsync(ct);
}
