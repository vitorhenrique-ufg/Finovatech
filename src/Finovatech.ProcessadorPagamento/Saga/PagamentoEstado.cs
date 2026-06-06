using MassTransit;

namespace Finovatech.ProcessadorPagamento.Saga;

public class PagamentoEstado : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid PagamentoId { get; set; }
    public decimal Valor { get; set; }
    public string MoedaOrigem { get; set; } = string.Empty;
    public string MoedaDestino { get; set; } = string.Empty;
    public string ParceiroOrigemId { get; set; } = string.Empty;
    public string ParceiroDestinoId { get; set; } = string.Empty;
    public DateTimeOffset IniciadoEm { get; set; }
}
