using Arquitetura.SharedKernel.Primitives;
using Produtos.Domain.Exceptions;

namespace Produtos.Domain.Entities;

public sealed class Produto : Entity
{
    private Produto() { } // EF Core

    private Produto(Guid id, string nome, string descricao, decimal preco, int estoque)
        : base(id)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public static Produto Criar(string nome, string descricao, decimal preco, int estoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ProdutoException("O nome do produto é obrigatório.");
        if (preco <= 0)
            throw new ProdutoException("O preço deve ser maior que zero.");
        if (estoque < 0)
            throw new ProdutoException("O estoque não pode ser negativo.");

        return new Produto(Guid.NewGuid(), nome.Trim(), descricao.Trim(), preco, estoque);
    }

    public void Atualizar(string nome, string descricao, decimal preco, int estoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ProdutoException("O nome do produto é obrigatório.");
        if (preco <= 0)
            throw new ProdutoException("O preço deve ser maior que zero.");
        if (estoque < 0)
            throw new ProdutoException("O estoque não pode ser negativo.");

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
        Estoque = estoque;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Debita a quantidade do estoque após um pedido confirmado.
    /// </summary>
    public void DebitarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ProdutoException("A quantidade a debitar deve ser maior que zero.");
        if (Estoque < quantidade)
            throw new ProdutoException($"Estoque insuficiente para o produto '{Nome}'. Disponível: {Estoque}, solicitado: {quantidade}.");

        Estoque -= quantidade;
        AtualizadoEm = DateTime.UtcNow;
    }
}
