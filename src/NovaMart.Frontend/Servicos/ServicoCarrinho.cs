using NovaMart.Frontend.Modelos;

namespace NovaMart.Frontend.Servicos;

public class ServicoCarrinho : IServicoCarrinho
{
    private readonly List<ItemCarrinhoViewModel> _itens = [];
    private string? _cupom;

    public IReadOnlyList<ItemCarrinhoViewModel> Itens => _itens.AsReadOnly();
    public int TotalItens => _itens.Sum(i => i.Quantidade);
    public decimal Subtotal => _itens.Sum(i => i.Total);
    public decimal Desconto => _cupom == "NOVATECH10" ? Math.Round(Subtotal * 0.10m, 2) : 0m;
    public decimal TotalComDesconto => Subtotal - Desconto;

    public event EventHandler? OnCarrinhoAlterado;

    public void AdicioneItem(ProdutoViewModel produto)
    {
        ItemCarrinhoViewModel? existente = _itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (existente is not null)
        {
            existente.Quantidade++;
        }
        else
        {
            _itens.Add(new ItemCarrinhoViewModel(produto.Id, produto.Nome, produto.Preco));
        }

        OnCarrinhoAlterado?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(Guid produtoId)
    {
        ItemCarrinhoViewModel? item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is not null)
        {
            _itens.Remove(item);
            OnCarrinhoAlterado?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AtualizequantidadeItem(Guid produtoId, int quantidade)
    {
        ItemCarrinhoViewModel? item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is not null)
        {
            item.Quantidade = Math.Max(1, quantidade);
            OnCarrinhoAlterado?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ApliceCupom(string cupom)
    {
        _cupom = cupom.ToUpperInvariant();
        OnCarrinhoAlterado?.Invoke(this, EventArgs.Empty);
    }

    public void LimpeCarrinho()
    {
        _itens.Clear();
        _cupom = null;
        OnCarrinhoAlterado?.Invoke(this, EventArgs.Empty);
    }
}
