import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pedidosService } from '@/services/pedidosService';
import type { CriarPedidoRequest } from '@/types/pedido';

export const pedidosKeys = {
  cliente: (clienteId: string) => ['pedidos', 'cliente', clienteId] as const,
  detail: (id: string) => ['pedidos', id] as const,
};

export function usePedidosPorCliente(clienteId: string) {
  return useQuery({
    queryKey: pedidosKeys.cliente(clienteId),
    queryFn: () => pedidosService.listarPorCliente(clienteId),
    enabled: !!clienteId,
  });
}

export function usePedido(id: string) {
  return useQuery({
    queryKey: pedidosKeys.detail(id),
    queryFn: () => pedidosService.obterPorId(id),
    enabled: !!id,
  });
}

export function useCriarPedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarPedidoRequest) => pedidosService.criar(payload),
    onSuccess: (_data, vars) => {
      queryClient.invalidateQueries({ queryKey: pedidosKeys.cliente(vars.clienteId) });
    },
  });
}
