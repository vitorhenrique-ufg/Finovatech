namespace Finovatech.DeteccaoFraude.Dominio.Interfaces;

public interface IClienteIA
{
    bool EstaHabilitado { get; }

    string ProvedorAtivo { get; }

    Task<string?> GeraRespostaJsonAsync(
        string sistemaPrompt,
        string userPrompt,
        CancellationToken ct = default);
}
