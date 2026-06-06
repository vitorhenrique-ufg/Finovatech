namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;

public record ConsultePagamentoConsulta(Guid PagamentoId);

public record ConsultePagamentoResultado(
    Guid PagamentoId,
    string MoedaOrigem,
    string MoedaDestino,
    decimal Valor,
    string Situacao,
    DateTimeOffset CriadoEm);
