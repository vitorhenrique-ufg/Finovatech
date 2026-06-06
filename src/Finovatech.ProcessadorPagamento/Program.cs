using Finovatech.ProcessadorPagamento.Aplicacao.ProcesseAnaliseFraudeConcluida;
using Finovatech.ProcessadorPagamento.Aplicacao.ProcessePagamentoIniciado;
using Finovatech.ProcessadorPagamento.Infraestrutura.Mensageria;
using Finovatech.ProcessadorPagamento.Saga;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessePagamentoIniciadoConsumidor>();
    x.AddConsumer<ProcesseAnaliseFraudeConcluidaConsumidor>();

    x.AddSagaStateMachine<PagamentoStateMachine, PagamentoEstado>()
        .InMemoryRepository();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("pagamento.iniciado", e =>
        {
            e.UseMessageRetry(r =>
                r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<ProcessePagamentoIniciadoConsumidor>(ctx);
        });

        cfg.ReceiveEndpoint("analise-fraude.concluida", e =>
        {
            e.UseMessageRetry(r =>
                r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<ProcesseAnaliseFraudeConcluidaConsumidor>(ctx);
        });

        cfg.ReceiveEndpoint("pagamento.saga", e =>
        {
            e.StateMachineSaga<PagamentoEstado>(ctx);
        });
    });
});

builder.Services.AddScoped<IProcessePagamentoIniciadoHandler, ProcessePagamentoIniciadoHandler>();
builder.Services.AddScoped<IProcesseAnaliseFraudeConcluidaHandler, ProcesseAnaliseFraudeConcluidaHandler>();

builder.Services.AddHealthChecks();

string serviceName = "finovatech-processador-pagamento";
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

app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

app.Run();
