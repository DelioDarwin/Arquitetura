import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { produtosService } from '@/services/produtosService';
import type { CriarProdutoRequest, AtualizarProdutoRequest } from '@/types/produto';

export const produtosKeys = {
  all: ['produtos'] as const,
  detail: (id: string) => ['produtos', id] as const,
};

export function useProdutos() {
  return useQuery({
    queryKey: produtosKeys.all,
    queryFn: produtosService.listar,
  });
}

export function useProduto(id: string) {
  return useQuery({
    queryKey: produtosKeys.detail(id),
    queryFn: () => produtosService.obterPorId(id),
    enabled: !!id,
  });
}

export function useCriarProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarProdutoRequest) => produtosService.criar(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: produtosKeys.all }),
  });
}

export function useAtualizarProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...payload }: AtualizarProdutoRequest & { id: string }) =>
      produtosService.atualizar(id, payload),
    onSuccess: (_data, vars) => {
      queryClient.invalidateQueries({ queryKey: produtosKeys.all });
      queryClient.invalidateQueries({ queryKey: produtosKeys.detail(vars.id) });
    },
  });
}

export function useExcluirProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => produtosService.excluir(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: produtosKeys.all }),
  });
}
