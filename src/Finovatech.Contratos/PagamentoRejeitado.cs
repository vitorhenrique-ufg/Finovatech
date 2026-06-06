namespace Finovatech.Contratos;

public record PagamentoRejeitado : EventoBase
{
    public required Guid PagamentoId { get; init; }
    public required string Motivo { get; init; }
    public required string ParceiroOrigemId { get; init; }
    public string? UsuarioId { get; init; }
}
