using Anthropic;
using Anthropic.Models.Messages;
using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.Servicos;

namespace Finovatech.DeteccaoFraude.Infraestrutura.IA;

public class ClienteIAAnthropicAdapter(
    AnthropicClient cliente,
    ConfiguracaoProvedorIA config,
    ILogger<ClienteIAAnthropicAdapter> logger) : IClienteIA
{
    public bool EstaHabilitado => true;
    public string ProvedorAtivo => $"Anthropic/{config.Modelo}";

    public async Task<string?> GeraRespostaJsonAsync(
        string sistemaPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        MessageCreateParams parametros = new()
        {
            Model = config.Modelo,
            MaxTokens = 1024,
            System = sistemaPrompt,
            Messages = [new() { Role = Role.User, Content = userPrompt }],
        };

        Message resposta = await cliente.Messages.Create(parametros).WaitAsync(ct);

        string? json = resposta.Content
            .Select(b => b.Value)
            .OfType<TextBlock>()
            .FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(json))
        {
            logger.LogWarning("[Anthropic] Resposta vazia do modelo {Modelo}", config.Modelo);
        }

        return json;
    }
}
