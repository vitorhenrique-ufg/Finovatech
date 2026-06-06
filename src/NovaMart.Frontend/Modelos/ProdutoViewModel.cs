namespace NovaMart.Frontend.Modelos;

public record ProdutoViewModel(
    Guid Id,
    string Nome,
    string Categoria,
    decimal Preco,
    decimal? PrecoOriginal,
    int Estoque,
    string Descricao,
    string ImagemUrl = "",
    IReadOnlyList<string>? GaleriaUrls = null
);
