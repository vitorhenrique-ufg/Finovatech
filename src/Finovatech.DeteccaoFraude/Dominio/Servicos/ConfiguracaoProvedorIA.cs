namespace Finovatech.DeteccaoFraude.Dominio.Servicos;

public record ConfiguracaoProvedorIA
{
    public string Provedor { get; init; } = "";

    public string Modelo { get; init; } = "";

    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Modelo) &&
        (!string.IsNullOrWhiteSpace(ApiKey) || Provedor.Equals("Ollama", StringComparison.OrdinalIgnoreCase));
}
