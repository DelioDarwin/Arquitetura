export interface CriarClienteRequest {
  nome: string;
  email: string;
  cpf: string;
}

export interface AtualizarClienteRequest {
  nome: string;
  email: string;
  cpf: string;
}

export interface ClienteDetalhe {
  clienteId: string;
  nome: string;
  email: string;
  cpf: string;
}

export interface Cliente {
  clienteId: string;
  nome: string;
  email: string;
  cpf: string;
  criadoEm: string;
  atualizadoEm?: string;
}

export interface ClienteCriado {
  clienteId: string;
}
