using Finovatech.DeteccaoFraude.Dominio.ObjetosDeValor;

namespace Finovatech.DeteccaoFraude.Dominio.Interfaces;

public interface IServicoAnaliseFraudeIA
{
    bool IAHabilitada { get; }

    Task<ResultadoAnalise?> AnaliseAsync(
        decimal valor,
        string moedaOrigem,
        string moedaDestino,
        string parceiroOrigemId,
        string parceiroDestinoId,
        Guid correlacaoId,
        CancellationToken ct = default);
}
