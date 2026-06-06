namespace Finovatech.GatewayPagamento.Api.Modelos;

public record CriarPagamentoRequest(
    string MoedaOrigem,
    string MoedaDestino,
    decimal Valor,
    string ParceiroOrigemId,
    string ParceiroDestinoId);
