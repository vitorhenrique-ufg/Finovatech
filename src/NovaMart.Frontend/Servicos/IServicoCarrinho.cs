using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public interface IServicoCarrinho
{
    IReadOnlyList<ItemCarrinhoViewModel> Itens { get; }
    int TotalItens { get; }
    decimal Subtotal { get; }
    decimal Desconto { get; }
    decimal TotalComDesconto { get; }

    event EventHandler? OnCarrinhoAlterado;

    void AdicioneItem(ProdutoViewModel produto);
    void RemoveItem(Guid produtoId);
    void AtualizequantidadeItem(Guid produtoId, int quantidade);
    void ApliceCupom(string cupom);
    void LimpeCarrinho();
}
