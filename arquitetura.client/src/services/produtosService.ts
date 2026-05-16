import { produtosApi } from '@/lib/http';
import type {
  Produto,
  CriarProdutoRequest,
  AtualizarProdutoRequest,
} from '@/types/produto';

export const produtosService = {
  async listar(): Promise<Produto[]> {
    const { data } = await produtosApi.get<Produto[]>('/');
    return data;
  },

  async obterPorId(id: string): Promise<Produto> {
    const { data } = await produtosApi.get<Produto>(`/${id}`);
    return data;
  },

  async criar(payload: CriarProdutoRequest): Promise<Produto> {
    const { data } = await produtosApi.post<Produto>('/', payload);
    return data;
  },

  async atualizar(id: string, payload: AtualizarProdutoRequest): Promise<void> {
    await produtosApi.put(`/${id}`, payload);
  },

  async excluir(id: string): Promise<void> {
    await produtosApi.delete(`/${id}`);
  },
};
