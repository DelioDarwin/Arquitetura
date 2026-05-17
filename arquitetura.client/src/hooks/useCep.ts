import { useQuery } from '@tanstack/react-query';
import { cepService } from '@/services/cepService';

export const cepKeys = {
  detail: (cep: string) => ['cep', cep] as const,
};

export function useCep(cep: string) {
  return useQuery({
    queryKey: cepKeys.detail(cep),
    queryFn: () => cepService.consultar(cep),
    enabled: cep.replace(/\D/g, '').length === 8,
    retry: false,
    staleTime: 1000 * 60 * 60, // 1 hora — CEP não muda
  });
}
