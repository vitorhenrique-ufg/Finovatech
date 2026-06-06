using Finovatech.ServicoCatalogo.Dados;
using Finovatech.ServicoCatalogo.Dominio;
using Finovatech.ServicoCatalogo.Endpoints;

namespace Finovatech.ServicoCatalogo.Testes.Endpoints;

public class FiltrarProdutosTestes
{
    private static IReadOnlyList<Produto> Todos => CatalogoDados.Produtos;

    [Fact] public void Filtre_SemFiltros_RetornaTodosProdutos() => Assert.Equal(20, ProdutosEndpoints.Filtre(Todos, null, null, null, null).Count);
    [Fact] public void Filtre_PorCategoria_RetornaApenasDaCategoria() { IReadOnlyList<Produto> r = ProdutosEndpoints.Filtre(Todos, "GPUs", null, null, null); Assert.Equal(5, r.Count); Assert.All(r, p => Assert.Equal("GPUs", p.Categoria)); }
    [Fact] public void Filtre_PorCategoria_IgnoraMaiusculas() => Assert.Equal(5, ProdutosEndpoints.Filtre(Todos, "gpus", null, null, null).Count);
    [Fact] public void Filtre_PorPrecoMinimo_RetornaProdutosAcimaDoLimite() => Assert.All(ProdutosEndpoints.Filtre(Todos, null, 2500m, null, null), p => Assert.True(p.Preco >= 2500m));
    [Fact] public void Filtre_PorPrecoMaximo_RetornaProdutosAbaixoDoLimite() => Assert.All(ProdutosEndpoints.Filtre(Todos, null, null, 500m, null), p => Assert.True(p.Preco <= 500m));
    [Fact] public void Filtre_PorBusca_RetornaProdutosComNomeContendo() => Assert.All(ProdutosEndpoints.Filtre(Todos, null, null, null, "RTX"), p => Assert.Contains("RTX", p.Nome, StringComparison.OrdinalIgnoreCase));
    [Fact] public void Filtre_PorBusca_IgnoraMaiusculas() => Assert.Equal(ProdutosEndpoints.Filtre(Todos, null, null, null, "RTX").Count, ProdutosEndpoints.Filtre(Todos, null, null, null, "rtx").Count);
    [Fact] public void Filtre_CategoriaInexistente_RetornaListaVazia() => Assert.Empty(ProdutosEndpoints.Filtre(Todos, "Monitores", null, null, null));
    [Fact] public void Filtre_TodosFiltros_RetornaSubconjunto() { IReadOnlyList<Produto> r = ProdutosEndpoints.Filtre(Todos, "GPUs", 1000m, 5000m, "RTX"); Assert.All(r, p => { Assert.Equal("GPUs", p.Categoria); Assert.True(p.Preco >= 1000m && p.Preco <= 5000m); Assert.Contains("RTX", p.Nome, StringComparison.OrdinalIgnoreCase); }); }
    [Fact] public void Filtre_BuscaSemResultado_RetornaListaVazia() => Assert.Empty(ProdutosEndpoints.Filtre(Todos, null, null, null, "XYZXYZ"));
}
