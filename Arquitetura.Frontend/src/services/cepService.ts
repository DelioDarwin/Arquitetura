import { cepApi } from '@/lib/http';
import type { ViaCep } from '@/types/cep';

export const cepService = {
  consultar: async (cep: string): Promise<ViaCep> => {
    const { data } = await cepApi.get<ViaCep>(`/${cep}`);
    return data;
  },
};
