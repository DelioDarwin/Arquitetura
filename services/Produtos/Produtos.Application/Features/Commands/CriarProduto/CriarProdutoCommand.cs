using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Produtos.Application.Features.Commands.CriarProduto;

public sealed record CriarProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result<Guid>>;
