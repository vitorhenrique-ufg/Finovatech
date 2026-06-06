using Finovatech.GatewayPagamento.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Finovatech.GatewayPagamento.Infraestrutura.Persistencia;

public class ContextoPagamento(DbContextOptions<ContextoPagamento> options) : DbContext(options)
{
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<RegistroOutbox> RegistrosOutbox => Set<RegistroOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pagamento>(entidade =>
        {
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.ChaveIdempotencia).IsRequired().HasMaxLength(100);
            entidade.Property(p => p.MoedaOrigem).IsRequired().HasMaxLength(3);
            entidade.Property(p => p.MoedaDestino).IsRequired().HasMaxLength(3);
            entidade.Property(p => p.Valor).HasPrecision(18, 4);
            entidade.Property(p => p.ParceiroOrigemId).IsRequired().HasMaxLength(100);
            entidade.Property(p => p.ParceiroDestinoId).IsRequired().HasMaxLength(100);
            entidade.Property(p => p.Situacao).HasConversion<string>().HasMaxLength(20);
            entidade.HasIndex(p => p.ChaveIdempotencia).IsUnique();
            entidade.ToTable("Pagamentos");
        });

        modelBuilder.Entity<RegistroOutbox>(entidade =>
        {
            entidade.HasKey(r => r.Id);
            entidade.Property(r => r.Tipo).IsRequired().HasMaxLength(100);
            entidade.Property(r => r.Carga).IsRequired();
            entidade.Property(r => r.Publicado).HasDefaultValue(false);
            entidade.HasIndex(r => r.Publicado);
            entidade.ToTable("RegistrosOutbox");
        });
    }
}
