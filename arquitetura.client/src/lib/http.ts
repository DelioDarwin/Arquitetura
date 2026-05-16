import axios from 'axios';
import type { ApiError } from '@/types/api';

export const produtosApi = axios.create({
  baseURL: '/api/produtos',
  headers: { 'Content-Type': 'application/json' },
});

export const pedidosApi = axios.create({
  baseURL: '/api/pedidos',
  headers: { 'Content-Type': 'application/json' },
});

export function extractApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error) && error.response?.data) {
    return error.response.data as ApiError;
  }
  return { title: 'Erro inesperado. Tente novamente.', status: 500 };
}
