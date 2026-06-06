using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finovatech.GatewayPagamento.Infraestrutura.Persistencia;

public class RepositorioOutbox(ContextoPagamento contexto) : IRepositorioOutbox
{
    public async Task AdicioneAsync(RegistroOutbox registro, CancellationToken ct = default)
        => await contexto.RegistrosOutbox.AddAsync(registro, ct);

    public async Task<IReadOnlyList<RegistroOutbox>> ConsultePendentesAsync(int limite = 50, CancellationToken ct = default)
        => await contexto.RegistrosOutbox
            .Where(r => !r.Publicado)
            .OrderBy(r => r.CriadoEm)
            .Take(limite)
            .ToListAsync(ct);

    public async Task SalveAlteracoesAsync(CancellationToken ct = default)
        => await contexto.SaveChangesAsync(ct);
}
