namespace Finovatech.GatewayPagamento.Api.Modelos;

public record PagamentoResponse(
    Guid PagamentoId,
    string Situacao,
    bool FoiCacheado = false);

public record PagamentoListaItem(
    Guid PagamentoId,
    string Situacao,
    decimal Valor,
    DateTimeOffset CriadoEm);

public record PagamentoDetalheResponse(
    Guid PagamentoId,
    string MoedaOrigem,
    string MoedaDestino,
    decimal Valor,
    string Situacao,
    DateTimeOffset CriadoEm);
