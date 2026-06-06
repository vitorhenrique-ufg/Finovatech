using System.Text.Json;
using Finovatech.ServicoNotificacao.Dominio.Entidades;
using Finovatech.ServicoNotificacao.Dominio.Interfaces;
using StackExchange.Redis;

namespace Finovatech.ServicoNotificacao.Infraestrutura.Push;

public class RepositorioAssinaturaPushRedis(IConnectionMultiplexer redis) : IRepositorioAssinaturaPush
{
    private static string Chave(string usuarioId) => $"push:sub:{usuarioId}";
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(60);

    public async Task SalveAsync(AssinaturaPush assinatura, CancellationToken ct = default)
    {
        IDatabase db = redis.GetDatabase();
        string json = JsonSerializer.Serialize(assinatura);
        await db.StringSetAsync(Chave(assinatura.UsuarioId), json, Ttl);
    }

    public async Task<AssinaturaPush?> ConsultePorUsuarioAsync(string usuarioId, CancellationToken ct = default)
    {
        IDatabase db = redis.GetDatabase();
        RedisValue valor = await db.StringGetAsync(Chave(usuarioId));
        if (valor.IsNullOrEmpty)
        {
            return null;
        }
        return JsonSerializer.Deserialize<AssinaturaPush>((string)valor!);
    }

    public async Task RemoveAsync(string usuarioId, CancellationToken ct = default)
    {
        IDatabase db = redis.GetDatabase();
        await db.KeyDeleteAsync(Chave(usuarioId));
    }
}
