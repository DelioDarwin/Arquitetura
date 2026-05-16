export interface Produto {
  id: string;
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
  criadoEm: string;
  atualizadoEm: string;
}

export interface CriarProdutoRequest {
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
}

export interface AtualizarProdutoRequest {
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
