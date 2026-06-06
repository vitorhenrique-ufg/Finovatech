using Finovatech.GatewayPagamento.Api.Endpoints;
using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.ConsultePagamento;
using Finovatech.GatewayPagamento.Aplicacao.CasosDeUso.CriePagamento;
using Finovatech.GatewayPagamento.Dominio.Interfaces;
using Finovatech.GatewayPagamento.Infraestrutura.Cache;
using Finovatech.GatewayPagamento.Infraestrutura.Mensageria;
using Finovatech.GatewayPagamento.Infraestrutura.Persistencia;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

string connString = builder.Configuration.GetConnectionString("Pagamentos")
    ?? "Host=localhost;Port=25432;Database=finovatech_pagamentos;Username=finovatech;Password=finovatech123";

builder.Services.AddDbContext<ContextoPagamento>(opts =>
    opts.UseNpgsql(connString));

builder.Services.AddScoped<IRepositorioPagamento, RepositorioPagamento>();
builder.Services.AddScoped<IRepositorioOutbox, RepositorioOutbox>();

string redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<IServicoIdempotencia, ServicoIdempotencia>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PagamentoAprovadoConsumidor>();
    x.AddConsumer<PagamentoRejeitadoConsumidor>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "finovatech");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "finovatech123");
        });

        cfg.ReceiveEndpoint("gateway-pagamento.resultado", e =>
        {
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            e.ConfigureConsumer<PagamentoAprovadoConsumidor>(ctx);
            e.ConfigureConsumer<PagamentoRejeitadoConsumidor>(ctx);
        });
    });
});

builder.Services.Configure<MassTransitHostOptions>(opts =>
{
    opts.WaitUntilStarted = false;
});

builder.Services.AddScoped<ICriePagamentoHandler, CriePagamentoHandler>();
builder.Services.AddScoped<IConsultePagamentoHandler, ConsultePagamentoHandler>();

builder.Services.AddHostedService<RelayOutbox>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ContextoPagamento>(name: "postgresql")
    .AddRedis(redisConn, name: "redis");

string serviceName = "finovatech-gateway-pagamento";
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

using (IServiceScope scope = app.Services.CreateScope())
{
    ContextoPagamento db = scope.ServiceProvider.GetRequiredService<ContextoPagamento>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/health");
app.MapPagamentosEndpoints();

app.Run();
