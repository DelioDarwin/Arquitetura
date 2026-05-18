using Arquitetura.SharedKernel.Common;
using Arquitetura.SharedKernel.Messaging;
using MediatR;
using Pedidos.Application.Abstractions;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Features.Commands.CriarPedido;

public sealed record CriarPedidoCommand(Guid ClienteId, List<ItemPedidoDto> Itens) : IRequest<Result<Guid>>;

public sealed record ItemPedidoDto(Guid ProdutoId, int Quantidade);

internal sealed class CriarPedidoCommandHandler(
    IPedidoRepository pedidoRepository,
    IProdutosServiceClient produtosClient,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher) : IRequestHandler<CriarPedidoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        var pedido = Pedido.Criar(request.ClienteId);

        foreach (var item in request.Itens)
        {
            var produto = await produtosClient.ObterProdutoAsync(item.ProdutoId, cancellationToken);
            if (produto is null)
                return Result.Failure<Guid>(new Error("Produto.NaoEncontrado", $"Produto '{item.ProdutoId}' não encontrado."));

            if (produto.Estoque < item.Quantidade)
                return Result.Failure<Guid>(new Error("Produto.EstoqueInsuficiente", $"Estoque insuficiente para o produto '{produto.Nome}'."));

            pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, item.Quantidade);
        }

        pedido.Confirmar();

        await pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publica o evento de integração para que outros serviços reajam
        // (ex: Produtos debita o estoque de forma assíncrona)
        var integrationEvent = new PedidoConfirmadoIntegrationEvent(
            EventId: Guid.NewGuid(),
            OcorridoEm: DateTime.UtcNow,
            PedidoId: pedido.Id,
            ClienteId: pedido.ClienteId,
            Itens: pedido.Itens
                .Select(i => new ItemPedidoConfirmado(i.ProdutoId, i.Quantidade))
                .ToList()
                .AsReadOnly());

        await eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return Result.Success(pedido.Id);
    }
}
