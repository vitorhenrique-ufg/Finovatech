namespace Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;

public record ResultadoAnalise
{
    public bool Aprovado { get; init; }
    public string? Motivo { get; init; }
    public decimal? NivelRisco { get; init; }
    public string? ResumoIA { get; init; }
    public bool AnalisadoPorIA { get; init; }

    public static ResultadoAnalise Aprovar() =>
        new() { Aprovado = true };

    public static ResultadoAnalise Aprovar(string resumoIA, decimal nivelRisco) =>
        new() { Aprovado = true, ResumoIA = resumoIA, NivelRisco = nivelRisco, AnalisadoPorIA = true };

    public static ResultadoAnalise Rejeitar(string motivo) =>
        new() { Aprovado = false, Motivo = motivo };

    public static ResultadoAnalise Rejeitar(string motivo, string resumoIA, decimal nivelRisco) =>
        new() { Aprovado = false, Motivo = motivo, ResumoIA = resumoIA, NivelRisco = nivelRisco, AnalisadoPorIA = true };
}
