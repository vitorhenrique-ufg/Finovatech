using Finovatech.ServicoCatalogo.Dados;
using Finovatech.ServicoCatalogo.Dominio;
using Microsoft.AspNetCore.Mvc;

namespace Finovatech.ServicoCatalogo.Endpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutos(this WebApplication app)
    {
        app.MapGet("/products", (
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search) =>
            Results.Ok(Filtre(CatalogoDados.Produtos, category, minPrice, maxPrice, search)));

        app.MapGet("/products/{id:guid}", (Guid id) =>
        {
            Produto? p = CatalogoDados.Produtos.FirstOrDefault(p => p.Id == id);
            return p is null ? Results.NotFound() : Results.Ok(p);
        });

        app.MapGet("/categories", () =>
            Results.Ok(CatalogoDados.Produtos.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToList()));
    }

    internal static IReadOnlyList<Produto> Filtre(
        IReadOnlyList<Produto> produtos, string? categoria,
        decimal? precoMinimo, decimal? precoMaximo, string? busca)
    {
        IEnumerable<Produto> q = produtos;
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            q = q.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
        }

        if (precoMinimo.HasValue)
        {
            q = q.Where(p => p.Preco >= precoMinimo.Value);
        }

        if (precoMaximo.HasValue)
        {
            q = q.Where(p => p.Preco <= precoMaximo.Value);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            q = q.Where(p => p.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase));
        }
        return q.ToList().AsReadOnly();
    }
}
