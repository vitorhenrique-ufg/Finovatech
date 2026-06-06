using Finovatech.ServicoCatalogo.Dados;

namespace Finovatech.ServicoCatalogo.Testes.Dados;

public class CatalogoDadosTestes
{
    [Fact] public void Produtos_DeveTerExatamente20Produtos() => Assert.Equal(20, CatalogoDados.Produtos.Count);
    [Fact] public void Produtos_DeveConterQuatroCategorias() => Assert.Equal(4, CatalogoDados.Produtos.Select(p => p.Categoria).Distinct().Count());
    [Fact] public void Produtos_DeveTerIdsUnicos() => Assert.Equal(20, CatalogoDados.Produtos.Select(p => p.Id).Distinct().Count());
    [Fact] public void Produtos_TodosDevemTerNomeNaoVazio() => Assert.All(CatalogoDados.Produtos, p => Assert.NotEmpty(p.Nome));
    [Fact] public void Produtos_TodosDevemTerPrecoPositivo() => Assert.All(CatalogoDados.Produtos, p => Assert.True(p.Preco > 0));
    [Fact] public void Produtos_Gpus_DeveTerCinco() => Assert.Equal(5, CatalogoDados.Produtos.Count(p => p.Categoria == "GPUs"));
    [Fact] public void Produtos_Processadores_DeveTerCinco() => Assert.Equal(5, CatalogoDados.Produtos.Count(p => p.Categoria == "Processadores"));
    [Fact] public void Produtos_Perifericos_DeveTerCinco() => Assert.Equal(5, CatalogoDados.Produtos.Count(p => p.Categoria == "Periféricos"));
    [Fact] public void Produtos_Licencas_DeveTerCinco() => Assert.Equal(5, CatalogoDados.Produtos.Count(p => p.Categoria == "Licenças"));
}
