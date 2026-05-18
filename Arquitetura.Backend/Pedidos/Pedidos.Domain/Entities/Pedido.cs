using Arquitetura.SharedKernel.Primitives;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public sealed class Pedido : Entity
{
    private readonly List<ItemPedido> _itens = [];

    private Pedido() { } // EF Core

    private Pedido(Guid id, Guid clienteId) : base(id)
    {
        ClienteId = clienteId;
        Status = StatusPedido.Pendente;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    public decimal Total => _itens.Sum(i => i.PrecoUnitario * i.Quantidade);

    public static Pedido Criar(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
            throw new PedidoException("O clienteId é obrigatório.");

        return new Pedido(Guid.NewGuid(), clienteId);
    }

    public void AdicionarItem(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        if (Status != StatusPedido.Pendente)
            throw new PedidoException("Só é possível adicionar itens em pedidos pendentes.");
        if (quantidade <= 0)
            throw new PedidoException("A quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new PedidoException("O preço unitário deve ser maior que zero.");

        var itemExistente = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente is not null)
        {
            _itens.Remove(itemExistente);
            _itens.Add(itemExistente with { Quantidade = itemExistente.Quantidade + quantidade });
        }
        else
        {
            _itens.Add(new ItemPedido(Guid.NewGuid(), Id, produtoId, nomeProduto, precoUnitario, quantidade));
        }

        AtualizadoEm = DateTime.UtcNow;
    }

    public void Confirmar()
    {
        if (Status != StatusPedido.Pendente)
            throw new PedidoException("Apenas pedidos pendentes podem ser confirmados.");
        if (_itens.Count == 0)
            throw new PedidoException("O pedido deve ter ao menos um item.");

        Status = StatusPedido.Confirmado;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status is StatusPedido.Entregue or StatusPedido.Cancelado)
            throw new PedidoException("Pedido já foi entregue ou cancelado.");

        Status = StatusPedido.Cancelado;
        AtualizadoEm = DateTime.UtcNow;
    }
}
