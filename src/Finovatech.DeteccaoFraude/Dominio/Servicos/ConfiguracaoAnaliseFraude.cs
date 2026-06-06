namespace Finovatech.DeteccaoFraude.Dominio.Servicos;

public record ConfiguracaoAnaliseFraude
{
    public decimal ValorMaximoPorTransacao { get; init; } = 50_000m;
    public IReadOnlyList<string> MoedasBloqueadas { get; init; } = [];
}
