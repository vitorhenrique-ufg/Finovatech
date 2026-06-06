using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public interface IServicoCatalogo
{
    Task<IReadOnlyList<ProdutoViewModel>> ListeProdutosAsync(
        string? categoria = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        string? busca = null);

    Task<ProdutoViewModel?> ConsulteProdutoAsync(Guid id);
    Task<IReadOnlyList<string>> ListeCategoriasAsync();
}
