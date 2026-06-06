using System.Net;
using System.Security.Claims;
using StackExchange.Redis;

namespace Finovatech.GatewayApi.RateLimiting;

public class MerchantRateLimitMiddleware(
    RequestDelegate next,
    IConnectionMultiplexer redis,
    ILogger<MerchantRateLimitMiddleware> logger)
{
    private const int LimiteRequisicoes = 200;
    private const int JanelaSegundos = 60;

    public async Task InvokeAsync(HttpContext context)
    {
        string? merchantId = ObtenhaMarketId(context);

        if (string.IsNullOrEmpty(merchantId))
        {
            await next(context);
            return;
        }

        IDatabase db = redis.GetDatabase();
        string chaveRedis = $"rate_limit:merchant:{merchantId}";
        long agora = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long inicioJanela = agora - (JanelaSegundos * 1000);

        ITransaction transacao = db.CreateTransaction();

        Task<long> removerAntigos = transacao.SortedSetRemoveRangeByScoreAsync(
            chaveRedis, double.NegativeInfinity, inicioJanela);

        Task<long> contarAtual = transacao.SortedSetLengthAsync(
            chaveRedis, inicioJanela, double.PositiveInfinity);

        await transacao.ExecuteAsync();

        long total = await contarAtual;

        if (total >= LimiteRequisicoes)
        {
            logger.LogWarning(
                "Rate limit atingido para merchantId {MerchantId}: {Total} requisições na janela",
                merchantId, total);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = JanelaSegundos.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = LimiteRequisicoes.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            await context.Response.WriteAsync("Rate limit por merchant excedido. Tente novamente em 1 minuto.");
            return;
        }

        await db.SortedSetAddAsync(chaveRedis, Guid.NewGuid().ToString(), agora);
        await db.KeyExpireAsync(chaveRedis, TimeSpan.FromSeconds(JanelaSegundos + 5));

        context.Response.Headers["X-RateLimit-Limit"] = LimiteRequisicoes.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (LimiteRequisicoes - total - 1).ToString();

        await next(context);
    }

    private static string? ObtenhaMarketId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Merchant-Id", out Microsoft.Extensions.Primitives.StringValues headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        string? subClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? context.User.FindFirstValue("sub");
        return subClaim;
    }
}
