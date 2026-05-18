export interface ItemPedido {
  produtoId: string;
  quantidade: number;
}

export interface CriarPedidoRequest {
  clienteId: string;
  itens: ItemPedido[];
}

export interface ItemPedidoDetalhe {
  produtoId: string;
  nomeProduto: string;
  quantidade: number;
  precoUnitario: number;
  subtotal: number;
}

export type StatusPedido = 'Pendente' | 'Confirmado' | 'Cancelado';

// Status numérico retornado pela API na listagem
export type StatusPedidoNumerico = 0 | 1 | 2;

export interface Pedido {
  id: string;
  clienteId?: string;
  status: StatusPedidoNumerico;
  total: number;
  itens?: ItemPedidoDetalhe[];
  criadoEm: string;
  atualizadoEm?: string;
}

export interface PedidoCriado {
  pedidoId: string;
}
