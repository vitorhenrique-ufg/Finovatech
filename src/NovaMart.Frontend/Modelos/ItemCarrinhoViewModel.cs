namespace NovaMart.Frontend.Modelos;

public class ItemCarrinhoViewModel(Guid produtoId, string nome, decimal preco)
{
    public Guid ProdutoId { get; init; } = produtoId;
    public string Nome { get; init; } = nome;
    public decimal Preco { get; init; } = preco;
    public int Quantidade { get; set; } = 1;
    public decimal Total => Preco * Quantidade;
}
