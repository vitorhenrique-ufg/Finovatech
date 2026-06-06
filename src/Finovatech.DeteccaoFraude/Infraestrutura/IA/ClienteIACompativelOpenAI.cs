using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.Servicos;
using System.Text.Json;

namespace Finovatech.DeteccaoFraude.Infraestrutura.IA;

public class ClienteIACompativelOpenAI(
    HttpClient httpClient,
    ConfiguracaoProvedorIA config,
    ILogger<ClienteIACompativelOpenAI> logger) : IClienteIA
{
    public bool EstaHabilitado => true;
    public string ProvedorAtivo => $"{config.Provedor}/{config.Modelo}";

    public async Task<string?> GeraRespostaJsonAsync(
        string sistemaPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        object corpo = new
        {
            model = config.Modelo,
            messages = new[]
            {
                new { role = "system", content = sistemaPrompt },
                new { role = "user",   content = userPrompt   },
            },
            response_format = new { type = "json_object" },
            max_tokens = 1024,
        };

        HttpResponseMessage resposta = await httpClient
            .PostAsJsonAsync("chat/completions", corpo, ct);

        if (!resposta.IsSuccessStatusCode)
        {
            string erro = await resposta.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "[{Provedor}] Erro HTTP {Status}: {Erro}",
                config.Provedor, (int)resposta.StatusCode, erro);
            return null;
        }

        string payload = await resposta.Content.ReadAsStringAsync(ct);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(payload);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[{Provedor}] Falha ao extrair conteúdo da resposta: {Payload}",
                config.Provedor, payload[..Math.Min(200, payload.Length)]);
            return null;
        }
    }
}
