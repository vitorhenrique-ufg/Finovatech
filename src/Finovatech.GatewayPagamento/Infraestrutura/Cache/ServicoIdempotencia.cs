using Finovatech.GatewayPagamento.Dominio.Interfaces;
using StackExchange.Redis;

namespace Finovatech.GatewayPagamento.Infraestrutura.Cache;

public class ServicoIdempotencia(IConnectionMultiplexer redis) : IServicoIdempotencia
{
    private readonly IDatabase _db = redis.GetDatabase();
    private const string Prefixo = "idempotencia:";

    public async Task<string?> ConsulteRespostaAsync(string chave, CancellationToken ct = default)
    {
        RedisValue valor = await _db.StringGetAsync(Prefixo + chave);
        return valor.HasValue ? valor.ToString() : null;
    }

    public async Task RegistreRespostaAsync(string chave, string resposta, TimeSpan ttl, CancellationToken ct = default)
        => await _db.StringSetAsync(Prefixo + chave, resposta, ttl);
}
