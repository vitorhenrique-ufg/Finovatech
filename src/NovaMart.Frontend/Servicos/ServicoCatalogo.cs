using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public class ServicoCatalogo(HttpClient httpClient) : IServicoCatalogo
{
    public async Task<IReadOnlyList<ProdutoViewModel>> ListeProdutosAsync(
        string? categoria = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        string? busca = null)
    {
        try
        {
            string url = "/catalog/products";
            List<string> parametros = [];

            if (!string.IsNullOrEmpty(categoria))
            {
                parametros.Add($"category={Uri.EscapeDataString(categoria)}");
            }

            if (precoMin.HasValue)
            {
                parametros.Add($"minPrice={precoMin}");
            }

            if (precoMax.HasValue)
            {
                parametros.Add($"maxPrice={precoMax}");
            }

            if (!string.IsNullOrEmpty(busca))
            {
                parametros.Add($"search={Uri.EscapeDataString(busca)}");
            }

            if (parametros.Count > 0)
            {
                url += "?" + string.Join("&", parametros);
            }

            HttpResponseMessage resposta = await httpClient.GetAsync(url);
            if (!resposta.IsSuccessStatusCode)
            {
                return [];
            }

            return await resposta.Content.ReadFromJsonAsync<List<ProdutoViewModel>>() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ProdutoViewModel?> ConsulteProdutoAsync(Guid id)
    {
        try
        {
            HttpResponseMessage resposta = await httpClient.GetAsync($"/catalog/products/{id}");
            if (!resposta.IsSuccessStatusCode)
            {
                return null;
            }
            return await resposta.Content.ReadFromJsonAsync<ProdutoViewModel>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<string>> ListeCategoriasAsync()
    {
        try
        {
            HttpResponseMessage resposta = await httpClient.GetAsync("/catalog/categories");
            if (!resposta.IsSuccessStatusCode)
            {
                return [];
            }
            return await resposta.Content.ReadFromJsonAsync<List<string>>() ?? [];
        }
        catch
        {
            return [];
        }
    }
}
