namespace Finovatech.Contratos;

public abstract record EventoBase
{
    public Guid CorrelacaoId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}
