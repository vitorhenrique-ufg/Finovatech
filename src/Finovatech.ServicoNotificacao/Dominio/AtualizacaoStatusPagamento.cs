namespace Finovatech.ServicoNotificacao.Dominio;

public record AtualizacaoStatusPagamento(
    Guid PagamentoId,
    string Situacao,
    string? Motivo,
    DateTimeOffset OcorridoEm
);
