namespace Finovatech.ServicoCatalogo.Dominio;

public record Produto(
    Guid Id,
    string Nome,
    string Categoria,
    decimal Preco,
    decimal? PrecoOriginal,
    int Estoque,
    string Descricao,
    string ImagemUrl,
    IReadOnlyList<string> GaleriaUrls
);
