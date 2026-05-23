import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientesService } from '@/services/clientesService';
import type { CriarClienteRequest, AtualizarClienteRequest } from '@/types/cliente';

export const clientesKeys = {
  all: ['clientes'] as const,
  detail: (id: string) => ['clientes', id] as const,
};

export function useClientes() {
  return useQuery({
    queryKey: clientesKeys.all,
    queryFn: clientesService.listar,
  });
}

export function useCliente(id: string) {
  return useQuery({
    queryKey: clientesKeys.detail(id),
    queryFn: () => clientesService.obterPorId(id),
    enabled: !!id,
  });
}

export function useCriarCliente() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarClienteRequest) => clientesService.criar(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientesKeys.all }),
  });
}

export function useAtualizarCliente() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...payload }: AtualizarClienteRequest & { id: string }) =>
      clientesService.atualizar(id, payload),
    onSuccess: (_data, vars) => {
      queryClient.invalidateQueries({ queryKey: clientesKeys.all });
      queryClient.invalidateQueries({ queryKey: clientesKeys.detail(vars.id) });
    },
  });
}

export function useExcluirCliente() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientesService.excluir(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientesKeys.all }),
  });
}
