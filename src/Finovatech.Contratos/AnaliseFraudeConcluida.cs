namespace Finovatech.Contratos;

public record AnaliseFraudeConcluida : EventoBase
{
    public required Guid PagamentoId { get; init; }
    public required bool Aprovado { get; init; }
    public string? MotivosRejeicao { get; init; }

    public required decimal Valor { get; init; }
    public required string MoedaOrigem { get; init; }
    public required string MoedaDestino { get; init; }
    public required string ParceiroOrigemId { get; init; }
    public required string ParceiroDestinoId { get; init; }
    public string? UsuarioId { get; init; }
}
