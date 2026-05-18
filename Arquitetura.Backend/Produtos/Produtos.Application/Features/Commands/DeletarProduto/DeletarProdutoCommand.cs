using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Produtos.Application.Features.Commands.DeletarProduto;

public sealed record DeletarProdutoCommand(Guid Id) : IRequest<Result>;
