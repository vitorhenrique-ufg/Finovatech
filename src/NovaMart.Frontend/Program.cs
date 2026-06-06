using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using NovaMart.Frontend.Components;
using NovaMart.Frontend.Servicos;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"))
    .SetApplicationName("novamart-frontend");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth";
        options.LogoutPath = "/api/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Serviços do frontend
builder.Services.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();
builder.Services.AddScoped<IServicoAssinaturaPush, ServicoAssinaturaPush>();
builder.Services.AddScoped<IServicoCarrinho, ServicoCarrinho>();
builder.Services.AddScoped<IServicoHubPedido, ServicoHubPedido>();
builder.Services.AddScoped<IServicoToast, ServicoToast>();

string gatewayUrl = builder.Configuration["GatewayApi:BaseUrl"] ?? "http://localhost:8000";
builder.Services.AddHttpClient<IServicoCatalogo, ServicoCatalogo>(c => c.BaseAddress = new Uri(gatewayUrl));
builder.Services.AddHttpClient("gateway", c => c.BaseAddress = new Uri(gatewayUrl));

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/auth/signin", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    string email = ctx.Request.Form["email"].ToString();
    string senha = ctx.Request.Form["senha"].ToString();
    string returnUrl = ctx.Request.Form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
    {
        return Results.Redirect("/auth?erro=preencha");
    }

    HttpClient client = factory.CreateClient("gateway");
    StringContent conteudo = new(JsonSerializer.Serialize(new { email, senha }),
                                 Encoding.UTF8,
                                 "application/json");

    HttpResponseMessage resposta = await client.PostAsync("/auth/login", conteudo);
    if (!resposta.IsSuccessStatusCode)
    {
        return Results.Redirect($"/auth?erro=credenciais&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    string json = await resposta.Content.ReadAsStringAsync();
    using JsonDocument doc = JsonDocument.Parse(json);
    string? token = doc.RootElement.GetProperty("accessToken").GetString();
    if (string.IsNullOrEmpty(token))
    {
        return Results.Redirect($"/auth?erro=credenciais&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    List<Claim> claims = ParseJwtClaims(token);
    claims.Add(new Claim("access_token", token));

    ClaimsIdentity identidade = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal usuario = new(identidade);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario,
        new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) });

    string destino = string.IsNullOrEmpty(returnUrl) ? "/catalog" : $"/{Uri.UnescapeDataString(returnUrl)}";
    return Results.Redirect(destino);
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/api/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
})
.DisableAntiforgery();

app.MapGet("/auth/sair", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/auth");
})
.AllowAnonymous();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

static List<Claim> ParseJwtClaims(string token)
{
    try
    {
        string[] partes = token.Split('.');
        if (partes.Length != 3)
        {
            return [];
        }

        string payload = partes[1];
        int padding = payload.Length % 4;
        if (padding != 0)
        {
            payload += new string('=', 4 - padding);
        }

        payload = payload.Replace('-', '+').Replace('_', '/');

        Dictionary<string, object>? claims =
            JsonSerializer.Deserialize<Dictionary<string, object>>(Convert.FromBase64String(payload));

        if (claims is null)
        {
            return [];
        }

        return
        [
            new Claim(ClaimTypes.NameIdentifier, claims.GetValueOrDefault("sub")?.ToString() ?? ""),
            new Claim(ClaimTypes.Email,           claims.GetValueOrDefault("email")?.ToString() ?? ""),
            new Claim(ClaimTypes.Name,            claims.GetValueOrDefault("email")?.ToString() ?? ""),
        ];
    }
    catch
    {
        return [];
    }
}
