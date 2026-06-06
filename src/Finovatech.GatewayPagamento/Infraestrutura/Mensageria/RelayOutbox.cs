using System.Text.Json;
using Finovatech.Contratos;
using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using MassTransit;

namespace Finovatech.GatewayPagamento.Infraestrutura.Mensageria;

public class RelayOutbox(
    IServiceScopeFactory scopeFactory,
    ILogger<RelayOutbox> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RelayOutbox iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesseLoteAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Erro ao processar lote do Outbox.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task ProcesseLoteAsync(CancellationToken ct)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IRepositorioOutbox repositorioOutbox = scope.ServiceProvider.GetRequiredService<IRepositorioOutbox>();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        IReadOnlyList<RegistroOutbox> pendentes = await repositorioOutbox.ConsultePendentesAsync(50, ct);

        foreach (RegistroOutbox registro in pendentes)
        {
            try
            {
                if (registro.Tipo == nameof(PagamentoIniciado))
                {
                    PagamentoIniciado? evento = JsonSerializer.Deserialize<PagamentoIniciado>(registro.Carga);
                    if (evento is not null)
                    {
                        await publishEndpoint.Publish(evento, ct);
                    }
                }

                registro.MarqueComoPublicado();
                logger.LogInformation("Outbox publicou {Tipo} id={Id}", registro.Tipo, registro.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao publicar registro Outbox {Id}.", registro.Id);
            }
        }

        if (pendentes.Count > 0)
        {
            await repositorioOutbox.SalveAlteracoesAsync(ct);
        }
    }
}
