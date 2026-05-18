import { pedidosApi } from '@/lib/http';
import type { Pedido, CriarPedidoRequest, PedidoCriado } from '@/types/pedido';

export const pedidosService = {
  async listarPorCliente(clienteId: string): Promise<Pedido[]> {
    const { data } = await pedidosApi.get<Pedido[]>(`/cliente/${clienteId}`);
    return data;
  },

  async obterPorId(id: string): Promise<Pedido> {
    const { data } = await pedidosApi.get<Pedido>(`/${id}`);
    return data;
  },

  async criar(payload: CriarPedidoRequest): Promise<PedidoCriado> {
    const { data } = await pedidosApi.post<PedidoCriado>('/', payload);
    return data;
  },
};
