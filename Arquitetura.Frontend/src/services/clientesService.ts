import { clientesApi } from '@/lib/http';
import type { Cliente, CriarClienteRequest, AtualizarClienteRequest, ClienteCriado } from '@/types/cliente';

export const clientesService = {
  async listar(): Promise<Cliente[]> {
    const { data } = await clientesApi.get<Cliente[]>('/');
    return data;
  },

  async obterPorId(clienteId: string): Promise<Cliente> {
    const { data } = await clientesApi.get<Cliente>(`/${clienteId}`);
    return data;
  },

  async criar(payload: CriarClienteRequest): Promise<ClienteCriado> {
    const { data } = await clientesApi.post('/', {
      ...payload,
      cpf: payload.cpf.replace(/\D/g, ''),
    });
    return data;
  },

  async atualizar(clienteId: string, payload: AtualizarClienteRequest): Promise<void> {
    await clientesApi.put(`/${clienteId}`, {
      ...payload,
      cpf: payload.cpf.replace(/\D/g, ''),
    });
  },

  async excluir(clienteId: string): Promise<void> {
    await clientesApi.delete(`/${clienteId}`);
  },
};
