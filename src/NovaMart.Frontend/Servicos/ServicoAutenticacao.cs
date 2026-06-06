using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;

namespace NovaMart.Frontend.Servicos;

public class ServicoAutenticacao(
    IHttpClientFactory factory,
    NavigationManager nav,
    IConfiguration configuration) : IServicoAutenticacao
{
    private string BaseUrl =>
        configuration["Frontend:BaseUrl"] ?? nav.BaseUri;

    public async Task<bool> EntreComCredenciaisAsync(string email, string senha)
    {
        HttpClient client = factory.CreateClient();
        client.BaseAddress = new Uri(BaseUrl);

        StringContent conteudo = new(JsonSerializer.Serialize(new { email, senha }),
                                     Encoding.UTF8,
                                     "application/json");

        HttpResponseMessage resposta = await client.PostAsync("/api/login", conteudo);
        return resposta.IsSuccessStatusCode;
    }

    public async Task EncerreSecaoAsync()
    {
        HttpClient client = factory.CreateClient();
        client.BaseAddress = new Uri(BaseUrl);
        await client.PostAsync("/api/logout", content: null);
    }
}
