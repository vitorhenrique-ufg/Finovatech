using Anthropic;
using Finovatech.DeteccaoFraude.Aplicacao.AnalisePagamento;
using Finovatech.DeteccaoFraude.Dominio.Interfaces;
using Finovatech.DeteccaoFraude.Dominio.Servicos;
using Finovatech.DeteccaoFraude.Infraestrutura.IA;
using Finovatech.DeteccaoFraude.Infraestrutura.Mensageria;
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

builder.Services.Configure<ConfiguracaoAnaliseFraude>(
    builder.Configuration.GetSection("AnaliseFraude"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AnalisePagamentoConsumidor>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("pagamento.enviado-para-analise", e =>
        {
            e.UseMessageRetry(r =>
                r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)));
            e.ConfigureConsumer<AnalisePagamentoConsumidor>(ctx);
        });
    });
});

builder.Services.AddScoped<IServicoAnaliseFraude, ServicoAnaliseFraude>();
builder.Services.AddScoped<IServicoAnaliseFraudeIA, ServicoAnaliseFraudeIA>();
builder.Services.AddScoped<IAnalisePagamentoHandler, AnalisePagamentoHandler>();

// ── Provedor de IA configurável ───────────────────────────────────────────────
ConfiguracaoProvedorIA configIA = builder.Configuration
    .GetSection("ProvedorIA")
    .Get<ConfiguracaoProvedorIA>() ?? new ConfiguracaoProvedorIA();

builder.Services.AddSingleton(configIA);

string provedor = configIA.Provedor.ToUpperInvariant();

if (!configIA.EstaConfigurado)
{
    // Sem configuração → Null Object: fluxo segue só pelas regras de negócio
    builder.Services.AddSingleton<IClienteIA, ClienteIADesabilitado>();
    builder.Services.AddLogging();
}
else if (provedor == "ANTHROPIC")
{
    builder.Services.AddSingleton(new AnthropicClient { ApiKey = configIA.ApiKey });
    builder.Services.AddScoped<IClienteIA, ClienteIAAnthropicAdapter>();
}
else
{
    // OpenRouter, OpenAI, Azure OpenAI, Groq, Ollama, LM Studio, etc.
    builder.Services.AddHttpClient<IClienteIA, ClienteIACompativelOpenAI>(client =>
    {
        client.BaseAddress = new Uri(configIA.BaseUrl.TrimEnd('/') + "/");

        if (!string.IsNullOrWhiteSpace(configIA.ApiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configIA.ApiKey}");
        }

        if (provedor == "OPENROUTER")
        {
            client.DefaultRequestHeaders.Add("HTTP-Referer", "https://finovatech.com");
            client.DefaultRequestHeaders.Add("X-Title", "FinovaTech DeteccaoFraude");
        }
    });
}
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddHealthChecks();

string serviceName = "finovatech-deteccao-fraude";
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
