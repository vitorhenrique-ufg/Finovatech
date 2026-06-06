using System.Text.Json;
using System.Text.Json.Serialization;
using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;

namespace Finovatech.DeteccaoFraude.Dominio.Servicos;

public class ServicoAnaliseFraudeIA(
    IClienteIA cliente,
    ILogger<ServicoAnaliseFraudeIA> logger) : IServicoAnaliseFraudeIA
{
    public bool IAHabilitada => cliente.EstaHabilitado;

    private const string SistemaPrompt = """
        Você é um sistema especializado em detecção de fraude financeira para pagamentos internacionais.
        Analise os dados do pagamento e determine se é legítimo ou potencialmente fraudulento.

        Critérios que elevam o risco:
        - Valores acima de 50.000 na moeda de origem são considerados de alto valor
        - Moedas de risco elevado: ZMW (Zâmbia), VES (Venezuela), IRR (Irã), MMK (Mianmar), KPW (Coreia do Norte)
        - IDs de parceiros com padrões suspeitos: prefixos "TEST-", "FAKE-", "TEMP-", "HACK-"
        - Conversões incomuns entre pares de moedas pouco líquidas
        - Valores exatamente redondos em grandes quantias podem indicar teste de sistema
        - Parceiros com mesma origem e destino são suspeitos em certas moedas

        Responda APENAS com JSON válido, sem markdown, sem blocos de código, no seguinte formato:
        {
          "aprovado": true|false,
          "nivelRisco": 0.0 a 1.0,
          "motivos": ["razão 1", "razão 2"],
          "resumo": "explicação detalhada em português"
        }
        """;

    private static readonly JsonSerializerOptions _opcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ResultadoAnalise?> AnaliseAsync(
        decimal valor,
        string moedaOrigem,
        string moedaDestino,
        string parceiroOrigemId,
        string parceiroDestinoId,
        Guid correlacaoId,
        CancellationToken ct = default)
    {
        string userPrompt = $"""
            Analise este pagamento internacional para detectar fraude:

            - Valor: {valor:N2} {moedaOrigem} → {moedaDestino}
            - ID do Parceiro de Origem: {parceiroOrigemId}
            - ID do Parceiro de Destino: {parceiroDestinoId}
            - ID de Correlação: {correlacaoId}
            """;

        try
        {
            string? json = await cliente.GeraRespostaJsonAsync(SistemaPrompt, userPrompt, ct);

            if (string.IsNullOrWhiteSpace(json))
            {
                logger.LogWarning(
                    "[{Provedor}] Resposta vazia para correlação {CorrelacaoId}",
                    cliente.ProvedorAtivo, correlacaoId);
                return null;
            }

            RespostaIA? respostaIA = JsonSerializer.Deserialize<RespostaIA>(json, _opcoesJson);
            if (respostaIA is null)
            {
                logger.LogWarning(
                    "[{Provedor}] Falha ao desserializar resposta para correlação {CorrelacaoId}: {Json}",
                    cliente.ProvedorAtivo, correlacaoId, json[..Math.Min(200, json.Length)]);
                return null;
            }

            logger.LogInformation(
                "[{Provedor}] Analisou {CorrelacaoId}: Aprovado={Aprovado}, Risco={Risco:P0} — {Resumo}",
                cliente.ProvedorAtivo, correlacaoId, respostaIA.Aprovado, respostaIA.NivelRisco, respostaIA.Resumo);

            return respostaIA.Aprovado
                ? ResultadoAnalise.Aprovar(respostaIA.Resumo, respostaIA.NivelRisco)
                : ResultadoAnalise.Rejeitar(
                    string.Join("; ", respostaIA.Motivos),
                    respostaIA.Resumo,
                    respostaIA.NivelRisco);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[{Provedor}] Falha ao analisar correlação {CorrelacaoId}",
                cliente.ProvedorAtivo, correlacaoId);
            return null;
        }
    }

    private record RespostaIA(
        [property: JsonPropertyName("aprovado")] bool Aprovado,
        [property: JsonPropertyName("nivelRisco")] decimal NivelRisco,
        [property: JsonPropertyName("motivos")] string[] Motivos,
        [property: JsonPropertyName("resumo")] string Resumo
    );
}
