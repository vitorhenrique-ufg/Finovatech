using Finovatech.ServicoNotificacao.Api.Endpoints;
using Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoAprovado;
using Finovatech.ServicoNotificacao.Aplicacao.NotifiquePagamentoRejeitado;
using Finovatech.ServicoNotificacao.Dominio.Interfaces;
using Finovatech.ServicoNotificacao.Infraestrutura.Hubs;
using Finovatech.ServicoNotificacao.Infraestrutura.Mensageria;
using Finovatech.ServicoNotificacao.Infraestrutura.Push;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, cfg) =>
    cfg
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code));

string rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
string rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "finovatech";
string rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "finovatech123";
string redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services
    .AddSignalR()
    .AddStackExchangeRedis(redisConn, options =>
    {
        options.Configuration.ChannelPrefix =
            new StackExchange.Redis.RedisChannel("finovatech", StackExchange.Redis.RedisChannel.PatternMode.Literal);
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PagamentoAprovadoConsumidor>();
    x.AddConsumer<PagamentoRejeitadoConsumidor>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
        cfg.ReceiveEndpoint("pagamento.aprovado", e =>
        {
            e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<PagamentoAprovadoConsumidor>(ctx);
        });
        cfg.ReceiveEndpoint("pagamento.rejeitado", e =>
        {
            e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<PagamentoRejeitadoConsumidor>(ctx);
        });
    });
});

builder.Services.AddScoped<INotifiquePagamentoAprovadoHandler, NotifiquePagamentoAprovadoHandler>();
builder.Services.AddScoped<INotifiquePagamentoRejeitadoHandler, NotifiquePagamentoRejeitadoHandler>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<IRepositorioAssinaturaPush, RepositorioAssinaturaPushRedis>();
builder.Services.AddScoped<IServicoNotificacaoPush, ServicoNotificacaoPushImpl>();

builder.Services
    .AddHealthChecks()
    .AddRedis(redisConn, name: "redis");

string serviceName = "finovatech-servico-notificacao";
string otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMassTransitInstrumentation()
            .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
    });

WebApplication app = builder.Build();

app.MapHub<StatusPagamentoHub>("/hubs/payments");
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");
app.MapNotificacoesEndpoints();

app.Run();
