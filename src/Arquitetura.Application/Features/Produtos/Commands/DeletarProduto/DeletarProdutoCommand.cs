using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.DeletarProduto;

public sealed record DeletarProdutoCommand(Guid Id) : IRequest<Result>;
