using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Produtos.Application.Features.Commands.AtualizarProduto;

public sealed record AtualizarProdutoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result>;
