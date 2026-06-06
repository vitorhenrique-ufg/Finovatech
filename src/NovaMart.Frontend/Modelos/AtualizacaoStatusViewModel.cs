namespace NovaMart.Frontend.Modelos;

public record AtualizacaoStatusViewModel(Guid PagamentoId, string Situacao, string? Motivo, DateTimeOffset OcorridoEm);
