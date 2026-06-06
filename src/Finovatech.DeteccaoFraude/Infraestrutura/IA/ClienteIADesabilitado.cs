using Finovatech.DeteccaoFraude.Dominio.Interfaces;

namespace Finovatech.DeteccaoFraude.Infraestrutura.IA;

public sealed class ClienteIADesabilitado : IClienteIA
{
    public bool EstaHabilitado => false;
    public string ProvedorAtivo => "Desabilitado";

    public Task<string?> GeraRespostaJsonAsync(
        string sistemaPrompt,
        string userPrompt,
        CancellationToken ct = default) => Task.FromResult<string?>(null);
}
