using Finovatech.Contratos;

namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;

public record CriePagamentoComando(
    string ChaveIdempotencia,
    string MoedaOrigem,
    string MoedaDestino,
    decimal Valor,
    string ParceiroOrigemId,
    string ParceiroDestinoId,
    string? UsuarioId = null);

public record CriePagamentoResultado(
    Guid PagamentoId,
    SituacaoPagamento Situacao,
    bool FoiCacheado = false);
