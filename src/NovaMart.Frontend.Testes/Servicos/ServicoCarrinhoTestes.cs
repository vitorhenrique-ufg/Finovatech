using NovaMart.Frontend.Modelos;
using NovaMart.Frontend.Servicos;

namespace NovaMart.Frontend.Testes.Servicos;

public class ServicoCarrinhoTestes
{
    private static ProdutoViewModel CrieProduto(decimal preco = 100m) => new(Guid.NewGuid(), "RTX 4080", "GPU", preco, null, 5, "Placa de vídeo");

    [Fact] public void AdicioneItem_DeveAdicionarProduto() { ServicoCarrinho c = new(); c.AdicioneItem(CrieProduto()); Assert.Single(c.Itens); }
    [Fact] public void AdicioneItem_DuasVezes_IncrementaQtd() { ServicoCarrinho c = new(); ProdutoViewModel p = CrieProduto(); c.AdicioneItem(p); c.AdicioneItem(p); Assert.Equal(2, c.Itens[0].Quantidade); }
    [Fact] public void RemoveItem_Remove() { ServicoCarrinho c = new(); ProdutoViewModel p = CrieProduto(); c.AdicioneItem(p); c.RemoveItem(p.Id); Assert.Empty(c.Itens); }
    [Fact] public void AtualizequantidadeItem_AlteraQtd() { ServicoCarrinho c = new(); ProdutoViewModel p = CrieProduto(); c.AdicioneItem(p); c.AtualizequantidadeItem(p.Id, 3); Assert.Equal(3, c.Itens[0].Quantidade); }
    [Fact] public void ApliceCupom_NOVATECH10_Desconto10Pct() { ServicoCarrinho c = new(); c.AdicioneItem(CrieProduto(1000m)); c.ApliceCupom("NOVATECH10"); Assert.Equal(900m, c.TotalComDesconto); }
    [Fact] public void TotalItens_SomaDasQtds() { ServicoCarrinho c = new(); c.AdicioneItem(CrieProduto()); c.AdicioneItem(CrieProduto()); Assert.Equal(2, c.TotalItens); }
}
