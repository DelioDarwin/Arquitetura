using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.AtualizarProduto;

public sealed record AtualizarProdutoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result>;
