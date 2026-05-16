using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.CriarProduto;

public sealed record CriarProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result<Guid>>;
