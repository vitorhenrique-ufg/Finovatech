using System.Diagnostics;
using System.Text.Json;
using Finovatech.Contratos;
using Finovatech.GatewayPagamento.Dominio.Entidades;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using Finovatech.GatewayPagamento.Dominio.ObjetosDeValor;

namespace Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;

public class CriePagamentoHandler(
    IRepositorioPagamento repositorioPagamento,
    IRepositorioOutbox repositorioOutbox,
    IServicoIdempotencia servicoIdempotencia) : ICriePagamentoHandler
{
    private static readonly TimeSpan TtlIdempotencia = TimeSpan.FromHours(24);

    public async Task<CriePagamentoResultado> ExecuteAsync(
        CriePagamentoComando comando,
        CancellationToken ct = default)
    {
        string? respostaCacheada = await servicoIdempotencia.ConsulteRespostaAsync(comando.ChaveIdempotencia, ct);
        if (respostaCacheada is not null)
        {
            CriePagamentoResultado cached = JsonSerializer.Deserialize<CriePagamentoResultado>(respostaCacheada)!;
            return cached with { FoiCacheado = true };
        }

        ChaveIdempotencia chave = new(comando.ChaveIdempotencia);
        Pagamento pagamento = Pagamento.Crie(
            chave,
            comando.MoedaOrigem,
            comando.MoedaDestino,
            comando.Valor,
            comando.ParceiroOrigemId,
            comando.ParceiroDestinoId,
            comando.UsuarioId);

        PagamentoIniciado evento = new()
        {
            PagamentoId = pagamento.Id,
            MoedaOrigem = pagamento.MoedaOrigem,
            MoedaDestino = pagamento.MoedaDestino,
            Valor = pagamento.Valor,
            ParceiroOrigemId = pagamento.ParceiroOrigemId,
            ParceiroDestinoId = pagamento.ParceiroDestinoId,
            UsuarioId = pagamento.UsuarioId,
        };

        Activity.Current?.SetTag("pagamento.correlacaoId", evento.CorrelacaoId.ToString());
        Activity.Current?.SetTag("pagamento.id", pagamento.Id.ToString());
        Activity.Current?.SetTag("pagamento.valor", evento.Valor.ToString("F2"));
        Activity.Current?.SetTag("pagamento.moeda", evento.MoedaOrigem);

        string cargaEvento = JsonSerializer.Serialize(evento);
        RegistroOutbox registroOutbox = RegistroOutbox.Crie(nameof(PagamentoIniciado), cargaEvento);

        await repositorioPagamento.AdicioneAsync(pagamento, ct);
        await repositorioOutbox.AdicioneAsync(registroOutbox, ct);
        await repositorioOutbox.SalveAlteracoesAsync(ct);

        CriePagamentoResultado resultado = new(pagamento.Id, pagamento.Situacao);
        string respostaJson = JsonSerializer.Serialize(resultado);
        await servicoIdempotencia.RegistreRespostaAsync(comando.ChaveIdempotencia, respostaJson, TtlIdempotencia, ct);

        return resultado;
    }
}
