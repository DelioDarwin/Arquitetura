using Arquitetura.SharedKernel.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;
using Produtos.Application.Abstractions;

namespace Produtos.Infrastructure.Messaging;

/// <summary>
/// Consome PedidoConfirmadoIntegrationEvent e debita o estoque de cada produto.
/// MassTransit garante entrega "at-least-once"; o guard de estoque em DebitarEstoque
/// evita operações inválidas em caso de reentrega.
/// </summary>
internal sealed class PedidoConfirmadoConsumer(
    IProdutoRepository produtoRepository,
    IUnitOfWork unitOfWork,
    ILogger<PedidoConfirmadoConsumer> logger) : IConsumer<PedidoConfirmadoIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PedidoConfirmadoIntegrationEvent> context)
    {
        var evento = context.Message;
        logger.LogInformation("Processando PedidoConfirmado {PedidoId} com {Count} item(ns).",
            evento.PedidoId, evento.Itens.Count);

        foreach (var item in evento.Itens)
        {
            var produto = await produtoRepository.ObterPorIdAsync(item.ProdutoId, context.CancellationToken);
            if (produto is null)
            {
                logger.LogWarning("Produto {ProdutoId} não encontrado ao processar pedido {PedidoId}.",
                    item.ProdutoId, evento.PedidoId);
                continue;
            }

            produto.DebitarEstoque(item.Quantidade);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Estoque atualizado para o pedido {PedidoId}.", evento.PedidoId);
    }
}
