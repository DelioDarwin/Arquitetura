# Documentacao do Frontend - arquitetura.client

> Guia de estudo detalhado que explica cada camada da interface, do clique do usuario
> ate a chamada HTTP a API e o retorno dos dados na tela. Cobre os dois CRUDs da
> aplicacao - **Produtos** e **Pedidos** - e destaca os componentes e padroes
> compartilhados entre eles.

---

## Sumario

1. [Visao Geral da Arquitetura Frontend](#1-visao-geral-da-arquitetura-frontend)
2. [Ponto de Entrada - main.tsx](#2-ponto-de-entrada--maintsx)
3. [Roteamento - router.ts](#3-roteamento--routerts)
4. [Shell da Aplicacao - Layout e Navbar](#4-shell-da-aplicacao--layout-e-navbar)
5. [Tipos TypeScript - O Contrato com a API](#5-tipos-typescript--o-contrato-com-a-api)
   - 5.1 [Tipos de Produto](#51-tipos-de-produto)
   - 5.2 [Tipos de Pedido](#52-tipos-de-pedido)
6. [Camada HTTP - lib/http.ts](#6-camada-http--libhttpts)
7. [Camadas de Servico](#7-camadas-de-servico)
   - 7.1 [produtosService.ts](#71-produtosservicets)
   - 7.2 [pedidosService.ts](#72-pedidosservicets)
8. [Camadas de Hooks](#8-camadas-de-hooks)
   - 8.1 [useProdutos.ts](#81-useprodutosts)
   - 8.2 [usePedidos.ts](#82-usepedidosts)
9. [Componentes UI Compartilhados](#9-componentes-ui-compartilhados)
   - 9.1 [Button](#91-button)
   - 9.2 [Input](#92-input)
   - 9.3 [Card, CardHeader, CardTitle, CardContent](#93-card-cardheader-cardtitle-cardcontent)
   - 9.4 [Badge](#94-badge)
   - 9.5 [EmptyState](#95-emptystate)
   - 9.6 [Spinner e PageSpinner](#96-spinner-e-pagespinner)
   - 9.7 [cn() - Classes condicionais](#97-cn--classes-condicionais)
10. [CRUD de Produtos - Pagina a Pagina](#10-crud-de-produtos--pagina-a-pagina)
    - 10.1 [Listar Produtos - ProdutosPage.tsx](#101-listar-produtos--produtospagetsx)
    - 10.2 [Criar Produto - NovoProdutoPage.tsx](#102-criar-produto--novoprodutopagetsx)
    - 10.3 [Editar Produto - EditarProdutoPage.tsx](#103-editar-produto--editarprodutopagetsx)
    - 10.4 [Excluir Produto - Fluxo inline na Lista](#104-excluir-produto--fluxo-inline-na-lista)
11. [CRUD de Pedidos - Pagina a Pagina](#11-crud-de-pedidos--pagina-a-pagina)
    - 11.1 [Listar Pedidos - PedidosPage.tsx](#111-listar-pedidos--pedidospagetsx)
    - 11.2 [Criar Pedido - NovoPedidoPage.tsx](#112-criar-pedido--novopedidopagetsx)
12. [Dashboard - DashboardPage.tsx](#12-dashboard--dashboardpagetsx)
13. [Fluxo Completo de Dados - Do Clique a API](#13-fluxo-completo-de-dados--do-clique-a-api)
    - 13.1 [Criar Produto](#131-criar-produto)
    - 13.2 [Criar Pedido](#132-criar-pedido)
14. [Utilitarios - lib/utils.ts](#14-utilitarios--libutilsts)
15. [Diagrama de Dependencias entre Camadas](#15-diagrama-de-dependencias-entre-camadas)

---

## 1. Visao Geral da Arquitetura Frontend

O frontend segue uma arquitetura em **cinco camadas horizontais**. Cada camada tem uma
responsabilidade unica e so conhece a camada imediatamente abaixo dela:

```
+-------------------------------------------------------+
|  PAGINAS  (src/pages/)                                |
|  Composicao completa de uma tela. Usa hooks,          |
|  componentes UI e navegacao.                          |
+-------------------------------------------------------+
|  HOOKS  (src/hooks/)                                  |
|  Estado de servidor com TanStack Query.               |
|  Cache, loading, erro, mutacoes e invalidacao.        |
+-------------------------------------------------------+
|  SERVICOS  (src/services/)                            |
|  Funcoes async puras que chamam Axios.                |
|  Nao conhecem React - podem ser testadas sozinhas.    |
+-------------------------------------------------------+
|  HTTP CLIENT  (src/lib/http.ts)                       |
|  Instancias Axios com baseURL e headers padrao.       |
|  O Proxy do Vite redireciona para as APIs reais.      |
+-------------------------------------------------------+
|  TIPOS  (src/types/)                                  |
|  Interfaces TypeScript que espelham os DTOs da API.   |
|  Garantem que o contrato com o backend e respeitado.  |
+-------------------------------------------------------+
```

**Componentes UI** (`src/components/`) sao uma camada transversal - elementos visuais
reutilizaveis sem logica de negocio, usados em qualquer pagina.

---

## 2. Ponto de Entrada - main.tsx

```
src/main.tsx
```

Este e o arquivo que o Vite executa primeiro. Ele monta toda a arvore de providers que
envolve a aplicacao.

```tsx
// src/main.tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from '@tanstack/react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { router } from './router';
import './index.css';

// 1. Cria o cliente de cache global do TanStack Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 30, // dados ficam "frescos" por 30 segundos
      retry: 1,             // tenta novamente 1x em caso de erro de rede
    },
  },
});

// 2. Monta a aplicacao no elemento <div id="root"> do index.html
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </StrictMode>
);
```

### O que cada provider faz

| Provider | Responsabilidade |
|----------|-----------------|
| `StrictMode` | Detecta efeitos colaterais e APIs depreciadas durante o desenvolvimento. Em producao nao tem efeito. |
| `QueryClientProvider` | Injeta o `queryClient` via Context, tornando o cache do TanStack Query acessivel em qualquer componente filho via hooks. |
| `RouterProvider` | Observa a URL do browser e renderiza o componente de pagina correspondente a rota atual. |
| `ReactQueryDevtools` | Painel flutuante que aparece apenas em `npm run dev`. Exibe o estado do cache, queries ativas e historico de mutacoes. Removido automaticamente no build de producao. |

---

## 3. Roteamento - router.ts

```
src/router.ts
```

O TanStack Router usa uma arvore de rotas construida manualmente em codigo. Cada rota
sabe qual componente renderizar e qual e seu pai na hierarquia.

```typescript
// src/router.ts

// createRootRoute define a rota raiz - sempre renderizada,
// envolve todas as outras com o RootLayout (navbar + main)
const rootRoute = createRootRoute({ component: RootLayout });

// Cada rota filha define:
//   path      -> segmento da URL que a ativa
//   component -> qual pagina renderizar
const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: DashboardPage,
});

const produtosRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/produtos',
  component: ProdutosPage,
});

const editarProdutoRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/produtos/$id/editar', // $id -> parametro dinamico capturado da URL
  component: EditarProdutoPage,
});

// A arvore conecta pai e filhos
const routeTree = rootRoute.addChildren([
  indexRoute,
  produtosRoute,
  novoProdutoRoute,
  editarProdutoRoute,
  pedidosRoute,
  novoPedidoRoute,
]);

export const router = createRouter({ routeTree });

// Registro de tipos - faz o TypeScript conhecer todas as rotas.
// Navegacao para rotas inexistentes gera erro em tempo de compilacao.
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
```

### Como a URL e mapeada para uma pagina

```
URL: /produtos/abc-123/editar
              |
              +-> capturado como parametro $id = "abc-123"
              |   ativa a rota /produtos/$id/editar
              |   renderiza EditarProdutoPage dentro do RootLayout
```

### Navegacao programatica nas paginas

```typescript
// Dentro de qualquer componente:
const navigate = useNavigate();

navigate({ to: '/produtos' });                              // sem parametros
navigate({ to: '/produtos/$id/editar', params: { id } }); // com parametro tipado
```

### Leitura de parametros de rota

```typescript
// Na pagina EditarProdutoPage:
const { id } = useParams({ strict: false }) as { id: string };
// id === "abc-123" quando a URL for /produtos/abc-123/editar
```

---

## 4. Shell da Aplicacao - Layout e Navbar

```
src/components/layout/RootLayout.tsx
src/components/layout/Navbar.tsx
```

### RootLayout

O `RootLayout` e o componente da `rootRoute`. Ele e renderizado em **todas** as paginas
e define a estrutura visual permanente da aplicacao.

```tsx
// src/components/layout/RootLayout.tsx
import { Outlet } from '@tanstack/react-router';
import { Navbar } from './Navbar';

export function RootLayout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <Outlet /> {/* aqui e renderizada a pagina da rota atual */}
      </main>
    </div>
  );
}
```

`<Outlet />` e o "buraco" onde o TanStack Router injeta o componente da rota ativa.
Quando a URL e `/produtos`, ele injeta `<ProdutosPage />`. Quando e `/produtos/novo`,
injeta `<NovoProdutoPage />`, e assim por diante.

### Navbar

```tsx
// src/components/layout/Navbar.tsx

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/produtos', label: 'Produtos', icon: Package },
  { to: '/pedidos', label: 'Pedidos', icon: ShoppingCart },
];

export function Navbar() {
  const { location } = useRouterState(); // observa a URL atual em tempo real

  return (
    <header className="sticky top-0 z-40 ...">
      {navItems.map(({ to, label, icon: Icon }) => {
        const isActive =
          to === '/'
            ? location.pathname === '/'
            : location.pathname.startsWith(to);

        return (
          <Link
            key={to}
            to={to}
            className={cn(
              'inline-flex items-center gap-1.5 ...',
              isActive ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 ...'
            )}
          >
            <Icon className="h-4 w-4" />
            {label}
          </Link>
        );
      })}
    </header>
  );
}
```

**Pontos importantes:**
- `useRouterState()` retorna o estado atual do router, incluindo `location.pathname`
- `location.pathname.startsWith('/produtos')` faz com que tanto `/produtos` quanto
  `/produtos/novo` e `/produtos/abc/editar` marquem "Produtos" como ativo
- `<Link to="/produtos">` gera um `<a>` que navega sem recarregar a pagina (SPA)

---

## 5. Tipos TypeScript - O Contrato com a API

Os tipos espelham exatamente os DTOs retornados pela API .NET. Se a API mudar um campo,
o TypeScript acusara o erro em todos os lugares que usam aquele tipo. Cada dominio tem
seu proprio arquivo de tipos.

### 5.1 Tipos de Produto

```
src/types/produto.ts
```

```typescript
// src/types/produto.ts

// Representa um produto retornado pela API (GET)
export interface Produto {
  id: string;        // Guid do backend -> string no TypeScript
  nome: string;
  descricao: string;
  preco: number;     // decimal do backend -> number no TypeScript
  estoque: number;   // int do backend -> number no TypeScript
  criadoEm: string;  // DateTime do backend -> string ISO 8601
  atualizadoEm: string;
}

// Payload enviado ao criar (POST) - sem id, criadoEm, atualizadoEm
export interface CriarProdutoRequest {
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
}

// Payload enviado ao atualizar (PUT) - mesmos campos do criar
export interface AtualizarProdutoRequest {
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
}
```

**Por que separar `Produto`, `CriarProdutoRequest` e `AtualizarProdutoRequest`?**
- `Produto` tem campos gerados pelo servidor (`id`, `criadoEm`) que nao devem ser
  enviados pelo cliente
- `CriarProdutoRequest` e `AtualizarProdutoRequest` definem exatamente o que o
  formulario precisa preencher
- Isso evita enviar dados desnecessarios e torna o contrato explicito

### 5.2 Tipos de Pedido

```
src/types/pedido.ts
```

```typescript
// src/types/pedido.ts

// Payload de um item ao criar pedido (POST)
export interface ItemPedido {
  produtoId: string;
  quantidade: number;
}

// Payload completo enviado ao criar um pedido
export interface CriarPedidoRequest {
  clienteId: string;  // UUID do cliente - validado como uuid('...') no Zod
  itens: ItemPedido[];
}

// Item detalhado retornado dentro de um Pedido (GET)
export interface ItemPedidoDetalhe {
  produtoId: string;
  nomeProduto: string;   // nome desnormalizado - retornado pela API
  quantidade: number;
  precoUnitario: number;
  subtotal: number;      // precoUnitario * quantidade
}

// Status numerico retornado pela API na listagem
// O backend serializa o enum como numero: 0 = Pendente, 1 = Confirmado, 2 = Cancelado
export type StatusPedidoNumerico = 0 | 1 | 2;

// Pedido completo retornado pela API (GET)
export interface Pedido {
  id: string;
  clienteId?: string;               // opcional - pode nao vir na listagem
  status: StatusPedidoNumerico;
  total: number;
  itens?: ItemPedidoDetalhe[];      // opcional - pode nao vir na listagem por cliente
  criadoEm: string;
  atualizadoEm?: string;
}

// Resposta da API ao criar um pedido (POST 201)
export interface PedidoCriado {
  pedidoId: string;  // apenas o id do pedido criado - sem o objeto completo
}
```

**Detalhe importante - `StatusPedidoNumerico`:** A API .NET serializa o enum
`StatusPedido` como inteiro por padrao. O frontend mapeia esses numeros para labels e
variantes de Badge usando tabelas de lookup:

```typescript
// PedidosPage.tsx
const statusLabel: Record<StatusPedidoNumerico, string> = {
  0: 'Pendente',
  1: 'Confirmado',
  2: 'Cancelado',
};

const statusVariant: Record<StatusPedidoNumerico, 'warning' | 'success' | 'destructive'> = {
  0: 'warning',
  1: 'success',
  2: 'destructive',
};
// Uso: <Badge variant={statusVariant[pedido.status]}>{statusLabel[pedido.status]}</Badge>
```

---

## 6. Camada HTTP - lib/http.ts

```
src/lib/http.ts
```

Esta camada cria as instancias do Axios pre-configuradas. **Nenhuma outra parte do
codigo importa o Axios diretamente** - sempre usam estas instancias.

```typescript
// src/lib/http.ts
import axios from 'axios';
import type { ApiError } from '@/types/api';

// Instancia dedicada para a API de Produtos
export const produtosApi = axios.create({
  baseURL: '/api/produtos',                          // prefixo de todas as URLs
  headers: { 'Content-Type': 'application/json' },  // header padrao em todas as requisicoes
});

// Instancia dedicada para a API de Pedidos
export const pedidosApi = axios.create({
  baseURL: '/api/pedidos',
  headers: { 'Content-Type': 'application/json' },
});

// Extrai a mensagem de erro padronizada de uma resposta de erro
export function extractApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error) && error.response?.data) {
    return error.response.data as ApiError;
  }
  return { title: 'Erro inesperado. Tente novamente.', status: 500 };
}
```

### O Proxy do Vite

Durante o desenvolvimento, quando `produtosApi.get('/')` e chamado, o Axios envia:

```
GET /api/produtos/
```

O Vite intercepta essa requisicao (por estar em `vite.config.ts`) e a encaminha para
a API real:

```
GET http://localhost:5001/api/produtos/
```

```
Browser (porta 5173)     Vite Dev Server         API .NET (porta 5001)
        |                       |                          |
        |--GET /api/produtos/--->|                          |
        |                       |--GET /api/produtos/------>|
        |                       |<---200 [array de produtos]|
        |<--200 [array de produtos]                         |
```

**Por que usar proxy?** Sem o proxy, o browser bloquearia a requisicao por **CORS**
(Cross-Origin Resource Sharing), ja que o cliente esta na porta 5173 e a API na 5001.
O proxy faz as requisicoes parecerem que vem da mesma origem.

### Configuracao do proxy em vite.config.ts

```typescript
server: {
  proxy: {
    '/api/produtos': { target: 'http://localhost:5001', changeOrigin: true },
    '/api/pedidos':  { target: 'http://localhost:5002', changeOrigin: true },
  }
}
```

---

## 7. Camadas de Servico

Os servicos sao **funcoes async puras**. Elas nao conhecem React, nao usam hooks e nao
gerenciam estado - apenas fazem chamadas HTTP e retornam dados tipados. Existem dois
servicos, um por dominio.

### 7.1 produtosService.ts

```
src/services/produtosService.ts
```

```typescript
// src/services/produtosService.ts
import { produtosApi } from '@/lib/http';
import type { Produto, CriarProdutoRequest, AtualizarProdutoRequest } from '@/types/produto';

export const produtosService = {

  // LIST -> GET /api/produtos/
  async listar(): Promise<Produto[]> {
    const { data } = await produtosApi.get<Produto[]>('/');
    return data;
  },

  // READ -> GET /api/produtos/{id}
  async obterPorId(id: string): Promise<Produto> {
    const { data } = await produtosApi.get<Produto>(`/${id}`);
    return data;
  },

  // CREATE -> POST /api/produtos/
  async criar(payload: CriarProdutoRequest): Promise<Produto> {
    const { data } = await produtosApi.post<Produto>('/', payload);
    return data;
  },

  // UPDATE -> PUT /api/produtos/{id}
  async atualizar(id: string, payload: AtualizarProdutoRequest): Promise<void> {
    await produtosApi.put(`/${id}`, payload);
    // A API retorna 204 No Content -> sem corpo para retornar
  },

  // DELETE -> DELETE /api/produtos/{id}
  async excluir(id: string): Promise<void> {
    await produtosApi.delete(`/${id}`);
    // A API retorna 204 No Content -> sem corpo para retornar
  },
};
```

### URL final de cada operacao

| Metodo | Chamada no Servico | URL completa enviada ao Vite | URL que chega na API |
|--------|-------------------|------------------------------|---------------------|
| `listar()` | `produtosApi.get('/')` | `GET /api/produtos/` | `GET http://localhost:5001/api/produtos/` |
| `obterPorId(id)` | `produtosApi.get('/abc')` | `GET /api/produtos/abc` | `GET http://localhost:5001/api/produtos/abc` |
| `criar(payload)` | `produtosApi.post('/', {...})` | `POST /api/produtos/` | `POST http://localhost:5001/api/produtos/` |
| `atualizar(id, payload)` | `produtosApi.put('/abc', {...})` | `PUT /api/produtos/abc` | `PUT http://localhost:5001/api/produtos/abc` |
| `excluir(id)` | `produtosApi.delete('/abc')` | `DELETE /api/produtos/abc` | `DELETE http://localhost:5001/api/produtos/abc` |

### 7.2 pedidosService.ts

```
src/services/pedidosService.ts
```

```typescript
// src/services/pedidosService.ts
import { pedidosApi } from '@/lib/http';
import type { Pedido, CriarPedidoRequest, PedidoCriado } from '@/types/pedido';

export const pedidosService = {

  // LIST por cliente -> GET /api/pedidos/cliente/{clienteId}
  async listarPorCliente(clienteId: string): Promise<Pedido[]> {
    const { data } = await pedidosApi.get<Pedido[]>(`/cliente/${clienteId}`);
    return data;
  },

  // READ -> GET /api/pedidos/{id}
  async obterPorId(id: string): Promise<Pedido> {
    const { data } = await pedidosApi.get<Pedido>(`/${id}`);
    return data;
  },

  // CREATE -> POST /api/pedidos/
  async criar(payload: CriarPedidoRequest): Promise<PedidoCriado> {
    const { data } = await pedidosApi.post<PedidoCriado>('/', payload);
    // A API retorna 201 Created + { pedidoId: "novo-guid" }
    return data;
  },
};
```

**Diferenca em relacao ao produtosService:**
- Pedidos nao tem operacoes de `atualizar` ou `excluir` expostas no frontend
- A listagem e sempre filtrada por `clienteId`
- A resposta do `criar` e um `PedidoCriado` (`{ pedidoId }`) e nao o pedido completo

| Metodo | Chamada no Servico | URL enviada ao Vite | URL na API |
|--------|-------------------|---------------------|-----------|
| `listarPorCliente(id)` | `pedidosApi.get('/cliente/abc')` | `GET /api/pedidos/cliente/abc` | `GET http://localhost:5002/api/pedidos/cliente/abc` |
| `obterPorId(id)` | `pedidosApi.get('/abc')` | `GET /api/pedidos/abc` | `GET http://localhost:5002/api/pedidos/abc` |
| `criar(payload)` | `pedidosApi.post('/', {...})` | `POST /api/pedidos/` | `POST http://localhost:5002/api/pedidos/` |

---

## 8. Camadas de Hooks

Os hooks conectam o React ao TanStack Query, adicionando **cache**, **estados de
carregamento**, **tratamento de erro** e **invalidacao automatica** sobre os servicos
da camada anterior.

### 8.1 useProdutos.ts

```
src/hooks/useProdutos.ts
```

```typescript
// src/hooks/useProdutos.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { produtosService } from '@/services/produtosService';
import type { CriarProdutoRequest, AtualizarProdutoRequest } from '@/types/produto';

// QUERY KEYS - chaves unicas que identificam cada "slot" no cache
export const produtosKeys = {
  all: ['produtos'] as const,
  detail: (id: string) => ['produtos', id] as const,
};

// Hook para listar todos os produtos
export function useProdutos() {
  return useQuery({
    queryKey: produtosKeys.all,
    queryFn: produtosService.listar,
  });
}
// Retorna: { data, isLoading, isFetching, isError, error, refetch, ... }

// Hook para buscar um unico produto pelo id
export function useProduto(id: string) {
  return useQuery({
    queryKey: produtosKeys.detail(id),
    queryFn: () => produtosService.obterPorId(id),
    enabled: !!id,  // so executa a query se id for nao-vazio
  });
}

// Hook para criar um produto
export function useCriarProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarProdutoRequest) => produtosService.criar(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: produtosKeys.all });
    },
  });
}

// Hook para atualizar um produto
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

// Hook para excluir um produto
export function useExcluirProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => produtosService.excluir(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: produtosKeys.all });
    },
  });
}
```

### Ciclo de vida do cache

```
Componente monta -> useQuery e chamado
        |
        v
Cache tem dados?
   -- NAO -> isLoading = true -> queryFn() -> dados chegam -> renderiza
   -- SIM -> renderiza imediatamente com dados do cache
                 |
                 v
          Dados estao stale? (>30s ou invalidados)
          -- NAO -> nao faz nova requisicao
          -- SIM -> isFetching = true (re-busca em background)
                        |
                        v
                   Dados chegam -> atualiza cache -> componente re-renderiza
```

### 8.2 usePedidos.ts

```
src/hooks/usePedidos.ts
```

```typescript
// src/hooks/usePedidos.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pedidosService } from '@/services/pedidosService';
import type { CriarPedidoRequest } from '@/types/pedido';

// QUERY KEYS
export const pedidosKeys = {
  cliente: (clienteId: string) => ['pedidos', 'cliente', clienteId] as const,
  detail: (id: string) => ['pedidos', id] as const,
};

// Hook para listar pedidos filtrados por clienteId
export function usePedidosPorCliente(clienteId: string) {
  return useQuery({
    queryKey: pedidosKeys.cliente(clienteId),
    queryFn: () => pedidosService.listarPorCliente(clienteId),
    enabled: !!clienteId,  // so executa se clienteId nao for vazio
  });
}

// Hook para buscar um pedido individual pelo id
export function usePedido(id: string) {
  return useQuery({
    queryKey: pedidosKeys.detail(id),
    queryFn: () => pedidosService.obterPorId(id),
    enabled: !!id,
  });
}

// Hook para criar um pedido
export function useCriarPedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarPedidoRequest) => pedidosService.criar(payload),
    onSuccess: (_data, vars) => {
      // Invalida somente o cache do cliente cujo pedido foi criado
      queryClient.invalidateQueries({
        queryKey: pedidosKeys.cliente(vars.clienteId),
      });
    },
  });
}
```

**Diferenca chave entre `useProdutos` e `usePedidos`:**

| Aspecto | useProdutos | usePedidos |
|---------|-------------|------------|
| Query principal | `useProdutos()` - lista global | `usePedidosPorCliente(id)` - lista filtrada |
| Chave de cache | `['produtos']` | `['pedidos', 'cliente', clienteId]` |
| Invalidacao em mutacao | Invalida toda a lista | Invalida apenas o cliente afetado |
| Operacoes de escrita | criar, atualizar, excluir | apenas criar |
| Resposta do criar | `Produto` completo | `PedidoCriado` (`{ pedidoId }`) |

---

## 9. Componentes UI Compartilhados

```
src/components/ui/
```

Todos os componentes UI sao **completamente independentes de dominio** - nao sabem se
estao numa tela de Produtos ou de Pedidos. Dois padroes fundamentais permeiam todos
eles: **`forwardRef`** e a funcao utilitaria **`cn()`**.

### 9.1 Button

```
src/components/ui/Button.tsx
```

```tsx
const variants = {
  primary:     'bg-indigo-600 text-white hover:bg-indigo-700 ...',
  secondary:   'bg-gray-100 text-gray-900 hover:bg-gray-200 ...',
  destructive: 'bg-red-600 text-white hover:bg-red-700 ...',
  ghost:       'text-gray-700 hover:bg-gray-100 ...',
  outline:     'border border-gray-300 text-gray-700 hover:bg-gray-50 ...',
};

const sizes = {
  sm: 'h-8 px-3 text-xs',
  md: 'h-9 px-4 text-sm',   // padrao
  lg: 'h-11 px-6 text-base',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, disabled, children, className, ...props }, ref) => {
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={cn(
          'inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors',
          variants[variant],
          sizes[size],
          (disabled || loading) && 'opacity-50 cursor-not-allowed',
          className
        )}
        {...props}
      >
        {loading && <svg className="animate-spin h-4 w-4">...</svg>}
        {children}
      </button>
    );
  }
);
```

**Onde e usado:**
- `variant="primary"` - Novo Produto, Novo Pedido, Criar/Salvar em formularios
- `variant="secondary"` - botao Buscar em PedidosPage
- `variant="destructive"` - confirmacao de exclusao em ProdutosPage
- `variant="ghost"` - botao voltar em NovoPedidoPage
- `variant="outline"` - "Adicionar item" em NovoPedidoPage
- `loading={isPending}` - qualquer botao de submit enquanto aguarda resposta da API

### 9.2 Input

```
src/components/ui/Input.tsx
```

```tsx
export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className, id, ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1">
        {label && <label htmlFor={id} className="text-sm font-medium text-gray-700">{label}</label>}
        <input
          ref={ref}   // forwardRef -> React Hook Form pode registrar o input
          id={id}
          className={cn(
            'h-9 w-full rounded-md border border-gray-300 px-3 text-sm',
            'focus:outline-none focus:ring-2 focus:ring-indigo-500',
            error && 'border-red-500 focus:ring-red-500',
            className
          )}
          {...props}
        />
        {error && <p className="text-xs text-red-600">{error}</p>}
      </div>
    );
  }
);
```

**Por que `forwardRef` e essencial aqui:** O `register('campo')` do React Hook Form
retorna um objeto com `{ ref, name, onChange, onBlur }`. Quando esse objeto e espalhado
(`{...register('campo')}`) em um componente customizado, a `ref` so chega ao `<input>`
nativo se o componente usar `forwardRef` para encaminhar a ref.

**Onde e usado:** em todos os formularios - `NovoProdutoPage`, `EditarProdutoPage`,
`NovoPedidoPage` e `PedidosPage`.

### 9.3 Card, CardHeader, CardTitle, CardContent

```
src/components/ui/Card.tsx
```

```tsx
// Card - container principal com borda e sombra
export function Card({ children, className }: CardProps) {
  return (
    <div className={cn('rounded-xl border border-gray-200 bg-white shadow-sm', className)}>
      {children}
    </div>
  );
}

// CardHeader - linha de cabecalho com separador inferior
export function CardHeader({ children, className }: CardProps) {
  return (
    <div className={cn('flex items-center justify-between px-6 py-4 border-b border-gray-100', className)}>
      {children}
    </div>
  );
}

// CardTitle - titulo semantico do card
export function CardTitle({ children, className }: CardProps) {
  return (
    <h3 className={cn('text-base font-semibold text-gray-900', className)}>{children}</h3>
  );
}

// CardContent - area de conteudo interno com padding padrao
export function CardContent({ children, className }: CardProps) {
  return <div className={cn('px-6 py-4', className)}>{children}</div>;
}
```

**Padrao de composicao:** o Card e construido compondo os quatro subcomponentes.
Exemplo em `NovoPedidoPage`:

```tsx
<Card>
  <CardHeader>
    <CardTitle>Itens do Pedido</CardTitle>
    <Button type="button" variant="outline" size="sm" onClick={...}>
      Adicionar item
    </Button>
  </CardHeader>
  <CardContent>
    {/* campos de itens */}
  </CardContent>
</Card>
```

**Onde e usado:** Dashboard, PedidosPage, NovoPedidoPage, NovoProdutoPage.

### 9.4 Badge

```
src/components/ui/Badge.tsx
```

```tsx
const variants = {
  default:     'bg-primary/10 text-primary',
  success:     'bg-emerald-100 text-emerald-700',   // estoque > 0, pedido Confirmado
  warning:     'bg-amber-100 text-amber-700',        // pedido Pendente
  destructive: 'bg-red-100 text-red-700',            // sem estoque, pedido Cancelado
  outline:     'border border-gray-300 text-gray-600',
};

export function Badge({ variant = 'default', children, className }: BadgeProps) {
  return (
    <span className={cn(
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
      variants[variant],
      className
    )}>
      {children}
    </span>
  );
}
```

**Onde e usado:**
- `ProdutosPage` - indica se o produto tem estoque (`success`) ou esta esgotado (`destructive`)
- `PedidosPage` - indica o status do pedido: Pendente (`warning`), Confirmado (`success`),
  Cancelado (`destructive`)

### 9.5 EmptyState

```
src/components/ui/EmptyState.tsx
```

```tsx
interface EmptyStateProps {
  title: string;
  description?: string;
  action?: React.ReactNode; // slot para botao de acao opcional
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="mb-4 rounded-full bg-gray-100 p-4">
        {/* icone generico */}
      </div>
      <p className="text-sm font-medium text-gray-900">{title}</p>
      {description && <p className="mt-1 text-sm text-gray-500">{description}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}
```

**Onde e usado:**
- `ProdutosPage` - quando nao ha produtos cadastrados
- `PedidosPage` - quando nenhum `clienteId` foi informado ainda e quando a busca nao
  retorna pedidos

### 9.6 Spinner e PageSpinner

```
src/components/ui/Spinner.tsx
```

```tsx
// Spinner inline - usado dentro de botoes e pequenas areas
export function Spinner({ className }: { className?: string }) {
  return (
    <svg className={`animate-spin h-5 w-5 text-indigo-600 ${className ?? ''}`} viewBox="0 0 24 24" fill="none">
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
    </svg>
  );
}

// PageSpinner - usado como conteudo principal enquanto a pagina carrega
export function PageSpinner() {
  return (
    <div className="flex h-64 items-center justify-center">
      <Spinner className="h-8 w-8" />
    </div>
  );
}
```

**Onde e usado:**
- `PageSpinner` - retornado diretamente em `ProdutosPage`, `EditarProdutoPage`,
  `DashboardPage` e `PedidosPage` durante `isLoading = true`
- `Spinner` embutido no `Button` - exibido automaticamente quando `loading={true}`

### 9.7 cn() - Classes condicionais

```
src/lib/utils.ts
```

```typescript
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

- **`clsx`**: combina classes condicionalmente
  ```ts
  cn('base', condition && 'extra', false && 'ignored') // -> 'base extra'
  ```
- **`twMerge`**: resolve conflitos entre classes Tailwind
  ```ts
  cn('p-4', 'p-8')                     // -> 'p-8'  (ultima vence)
  cn('text-red-500', 'text-blue-600')  // -> 'text-blue-600'
  ```

---

## 10. CRUD de Produtos - Pagina a Pagina

### 10.1 Listar Produtos - ProdutosPage.tsx

```
src/pages/produtos/ProdutosPage.tsx
```

**O que esta pagina faz:** Busca todos os produtos da API, renderiza cards com nome,
preco, estoque e acoes de editar/excluir.

```tsx
export function ProdutosPage() {
  const navigate = useNavigate();

  // Busca os produtos via TanStack Query
  const { data: produtos, isLoading, isError } = useProdutos();

  // Hook de mutacao para excluir
  const excluir = useExcluirProduto();

  // Estado local para o fluxo de confirmacao de exclusao
  const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

  if (isLoading) return <PageSpinner />;
  if (isError) return <p>Erro ao carregar produtos.</p>;

  return (
    <div>
      <Button onClick={() => navigate({ to: '/produtos/novo' })}>
        Novo Produto
      </Button>

      {!produtos || produtos.length === 0 ? (
        <EmptyState title="Nenhum produto" />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {produtos.map((produto) => (
            <Card key={produto.id}>
              <Badge variant={produto.estoque > 0 ? 'success' : 'destructive'}>
                {produto.estoque > 0 ? `${produto.estoque} un.` : 'Sem estoque'}
              </Badge>

              <Button
                onClick={() => navigate({
                  to: '/produtos/$id/editar',
                  params: { id: produto.id }
                })}
              >
                Editar
              </Button>

              {/* Fluxo de exclusao com confirmacao em dois passos */}
              {confirmDelete === produto.id ? (
                <Button
                  variant="destructive"
                  loading={excluir.isPending}
                  onClick={() =>
                    excluir.mutate(produto.id, {
                      onSuccess: () => setConfirmDelete(null),
                    })
                  }
                >
                  Confirmar
                </Button>
              ) : (
                <Button onClick={() => setConfirmDelete(produto.id)}>
                  <Trash2 />
                </Button>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Fluxo de dados do carregamento:**

```
1. ProdutosPage monta
2. useProdutos() e chamado
3. TanStack Query verifica o cache ['produtos']
4. Cache vazio -> isLoading = true -> componente renderiza <PageSpinner />
5. TanStack Query chama produtosService.listar()
6. Axios envia GET /api/produtos/ -> Vite Proxy -> GET http://localhost:5001/api/produtos/
7. API retorna: [{ id, nome, descricao, preco, estoque, criadoEm, atualizadoEm }, ...]
8. TanStack Query armazena no cache ['produtos']
9. isLoading = false, data = Produto[]
10. Componente re-renderiza com os cards dos produtos
```

### 10.2 Criar Produto - NovoProdutoPage.tsx

```
src/pages/produtos/NovoProdutoPage.tsx
```

**O que esta pagina faz:** Exibe um formulario validado pelo Zod. Ao submeter, chama a
API e redireciona para a lista.

```tsx
// Schema de validacao com Zod
const schema = z.object({
  nome:      z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  descricao: z.string().min(5, 'Descricao deve ter pelo menos 5 caracteres'),
  preco:     z.number().positive('Preco deve ser positivo'),
  estoque:   z.number().int().min(0, 'Estoque nao pode ser negativo'),
});

type FormData = z.infer<typeof schema>;

export function NovoProdutoPage() {
  const navigate = useNavigate();
  const criar = useCriarProduto();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) => {
    criar.mutate(data, {
      onSuccess: () => navigate({ to: '/produtos' }),
    });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input
        id="nome"
        label="Nome *"
        error={errors.nome?.message}
        {...register('nome')}
      />

      <Input
        id="preco"
        type="number"
        step="0.01"
        error={errors.preco?.message}
        {...register('preco', { valueAsNumber: true })}
        // valueAsNumber: true -> converte o valor de string para number
      />

      {criar.isError && (
        <p className="text-red-700">Erro ao criar produto.</p>
      )}

      <Button type="submit" loading={criar.isPending}>
        Criar Produto
      </Button>
    </form>
  );
}
```

**Fluxo de validacao e submissao:**

```
Usuario clica em "Criar Produto"
        |
        v
handleSubmit intercepta o evento submit
        |
        v
React Hook Form coleta os valores dos inputs registrados
        |
        v
zodResolver executa schema.parse(values)
   -- FALHA -> errors preenchidos -> formulario nao submete -> erros exibidos
   -- SUCESSO -> onSubmit(data) e chamado
                |
                v
        criar.mutate(data) -> TanStack Query
                |
                v
        produtosService.criar(data) -> POST /api/produtos/
                |
        +-----------------+
        | Sucesso (201)   | Erro (400/422/500)
        |                 |
  onSuccess()         isError = true
  invalidate cache    mensagem de erro exibida
  navigate('/produtos')
```

### 10.3 Editar Produto - EditarProdutoPage.tsx

```
src/pages/produtos/EditarProdutoPage.tsx
```

**O que esta pagina faz:** Le o `id` da URL, busca os dados atuais do produto, preenche
o formulario com eles, e ao submeter atualiza via API.

```tsx
export function EditarProdutoPage() {
  // Le o parametro $id da URL atual
  const { id } = useParams({ strict: false }) as { id: string };
  const navigate = useNavigate();

  // Busca o produto atual pelo id
  const { data: produto, isLoading } = useProduto(id);

  const atualizar = useAtualizarProduto();

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  // Preenche o formulario quando os dados do produto chegarem da API
  useEffect(() => {
    if (produto) {
      reset({
        nome: produto.nome,
        descricao: produto.descricao,
        preco: produto.preco,
        estoque: produto.estoque,
      });
    }
  }, [produto, reset]);

  const onSubmit = (data: FormData) => {
    atualizar.mutate(
      { id, ...data },
      { onSuccess: () => navigate({ to: '/produtos' }) }
    );
  };

  if (isLoading) return <PageSpinner />;

  return <form onSubmit={handleSubmit(onSubmit)}>...</form>;
}
```

**Por que `useEffect` com `reset`?**

```
Componente monta
        |
        v
useForm inicializa com campos vazios ("")
        |
        v
useProduto(id) -> isLoading = true
        |
        v
API responde com { nome: "Camiseta", preco: 99.90, ... }
        |
        v
useEffect detecta que produto mudou de undefined para Produto
        |
        v
reset({ nome: "Camiseta", preco: 99.90, ... })
        |
        v
Formulario re-renderiza com campos preenchidos
```

### 10.4 Excluir Produto - Fluxo inline na Lista

A exclusao nao tem pagina propria - acontece na propria `ProdutosPage` com um padrao
de confirmacao em dois cliques:

```tsx
const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

// Primeiro clique - apenas ativa o estado de confirmacao
<Button onClick={() => setConfirmDelete(produto.id)}>
  <Trash2 />
</Button>

// Segundo clique - efetivamente exclui
{confirmDelete === produto.id ? (
  <Button
    variant="destructive"
    loading={excluir.isPending}
    onClick={() =>
      excluir.mutate(produto.id, {
        onSuccess: () => setConfirmDelete(null),
      })
    }
  >
    Confirmar
  </Button>
) : (
  <Button onClick={() => setConfirmDelete(produto.id)}>
    <Trash2 />
  </Button>
)}
```

**Por que dois cliques?** Evita exclusoes acidentais. O primeiro clique apenas "arma" o
estado. O segundo clique confirma e dispara a mutacao.

---

## 11. CRUD de Pedidos - Pagina a Pagina

### 11.1 Listar Pedidos - PedidosPage.tsx

```
src/pages/pedidos/PedidosPage.tsx
```

**O que esta pagina faz:** Permite ao usuario informar um `clienteId` (UUID), buscar
todos os pedidos daquele cliente e visualiza-los em cards com status, total, data e itens.

```tsx
export function PedidosPage() {
  const navigate = useNavigate();

  // Dois estados locais separados - por que?
  // clienteId: o que o usuario esta digitando no campo
  // busca: o valor confirmado ao clicar em "Buscar"
  // Separar evita que a query seja re-executada a cada tecla digitada
  const [clienteId, setClienteId] = useState('');
  const [busca, setBusca] = useState('');

  // So executa quando busca !== ''
  const { data: pedidos, isLoading } = usePedidosPorCliente(busca);

  const statusLabel: Record<StatusPedidoNumerico, string> = {
    0: 'Pendente',
    1: 'Confirmado',
    2: 'Cancelado',
  };

  const statusVariant: Record<StatusPedidoNumerico, 'warning' | 'success' | 'destructive'> = {
    0: 'warning',
    1: 'success',
    2: 'destructive',
  };

  const handleBuscar = () => {
    setBusca(clienteId.trim());
  };

  return (
    <div>
      <Button onClick={() => navigate({ to: '/pedidos/novo' })}>
        Novo Pedido
      </Button>

      <Card>
        <CardContent>
          <Input
            id="clienteId"
            label="ID do Cliente (UUID)"
            placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            value={clienteId}
            onChange={(e) => setClienteId(e.target.value)}
          />
          <Button variant="secondary" onClick={handleBuscar}>
            Buscar
          </Button>
        </CardContent>
      </Card>

      {!busca && (
        <EmptyState
          title="Informe um ID de cliente"
          description="Digite o UUID do cliente e clique em Buscar para ver os pedidos."
        />
      )}

      {busca && isLoading && <PageSpinner />}

      {busca && !isLoading && pedidos?.length === 0 && (
        <EmptyState title="Nenhum pedido encontrado" description="Este cliente nao possui pedidos." />
      )}

      {pedidos && pedidos.length > 0 && (
        <div className="flex flex-col gap-4">
          {pedidos.map((pedido) => (
            <Card key={pedido.id}>
              <CardHeader>
                <Badge variant={statusVariant[pedido.status]}>
                  {statusLabel[pedido.status]}
                </Badge>
                <span>{formatCurrency(pedido.total)}</span>
              </CardHeader>
              <CardContent>
                <p>{formatDate(pedido.criadoEm)}</p>
                {(pedido.itens ?? []).map((item) => (
                  <div key={item.produtoId}>
                    <span>{item.nomeProduto}</span>
                    <span>{item.quantidade}x</span>
                    <span>{formatCurrency(item.subtotal)}</span>
                  </div>
                ))}
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Por que `clienteId` e `busca` sao estados separados?**

```
Sem separacao:
  Usuario digita "abc" -> query dispara 3x (a, ab, abc) - desperdicio de requisicoes

Com separacao:
  Usuario digita "abc-123-..." -> nenhuma query
  Usuario clica "Buscar" -> setBusca("abc-123-...") -> query dispara 1x
```

**Por que `pedido.itens ?? []`?**  
A API de listagem por cliente pode retornar pedidos sem a lista de itens detalhados
(otimizacao de payload). O operador `??` garante que, se `itens` for `undefined`, o
`.map()` receba um array vazio em vez de lancar um erro.

### 11.2 Criar Pedido - NovoPedidoPage.tsx

```
src/pages/pedidos/NovoPedidoPage.tsx
```

**O que esta pagina faz:** Exibe um formulario com `clienteId` + uma lista dinamica de
itens (produto + quantidade). Ao submeter, cria o pedido na API e exibe a tela de
sucesso com o `pedidoId` gerado.

```tsx
// Schema Zod - validacao de dois niveis: campos raiz + array de itens
const schema = z.object({
  clienteId: z.string().uuid('ID do cliente deve ser um UUID valido'),
  itens: z.array(
    z.object({
      produtoId: z.string().min(1, 'Selecione um produto'),
      quantidade: z.number().int().min(1, 'Quantidade minima e 1'),
    })
  ).min(1, 'Adicione pelo menos um item'),
});

type FormData = z.infer<typeof schema>;

export function NovoPedidoPage() {
  const navigate = useNavigate();
  const criar = useCriarPedido();
  const { data: produtos } = useProdutos();
  const [pedidoId, setPedidoId] = useState<string | null>(null);

  const { register, handleSubmit, control, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { itens: [{ produtoId: '', quantidade: 1 }] },
  });

  // useFieldArray gerencia o array de itens no formulario
  const { fields, append, remove } = useFieldArray({
    control,
    name: 'itens',
  });

  // watch e reativo - necessario para calcular o total estimado
  const itensWatch = watch('itens');

  const calcularTotal = () => {
    return itensWatch.reduce((acc, item) => {
      const produto = produtos?.find((p) => p.id === item.produtoId);
      return acc + (produto?.preco ?? 0) * (item.quantidade || 0);
    }, 0);
  };

  const onSubmit = (data: FormData) => {
    criar.mutate(data, {
      onSuccess: (res) => {
        setPedidoId(res.pedidoId);
      },
    });
  };

  // Tela de sucesso - exibida apos criar.mutate ter sucesso
  if (pedidoId) {
    return (
      <div>
        <p>Pedido criado com sucesso!</p>
        <p>ID: <code>{pedidoId}</code></p>
        <Button variant="outline" onClick={() => navigate({ to: '/pedidos' })}>
          Ver Pedidos
        </Button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input
        id="clienteId"
        label="ID do Cliente (UUID) *"
        placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        error={errors.clienteId?.message}
        {...register('clienteId')}
      />

      <Card>
        <CardHeader>
          <CardTitle>Itens do Pedido</CardTitle>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => append({ produtoId: '', quantidade: 1 })}
          >
            Adicionar item
          </Button>
        </CardHeader>
        <CardContent>
          {fields.map((field, index) => (
            <div key={field.id}>
              <select {...register(`itens.${index}.produtoId`)}>
                <option value="">Selecione um produto</option>
                {produtos?.map((p) => (
                  <option key={p.id} value={p.id}>{p.nome} - {formatCurrency(p.preco)}</option>
                ))}
              </select>

              <Input
                type="number"
                min="1"
                error={errors.itens?.[index]?.quantidade?.message}
                {...register(`itens.${index}.quantidade`, { valueAsNumber: true })}
              />

              {fields.length > 1 && (
                <Button type="button" variant="ghost" size="sm" onClick={() => remove(index)}>
                  <Trash2 />
                </Button>
              )}
            </div>
          ))}

          <p>Total estimado: <strong>{formatCurrency(calcularTotal())}</strong></p>
        </CardContent>
      </Card>

      <Button type="submit" loading={criar.isPending}>
        Criar Pedido
      </Button>
    </form>
  );
}
```

**Como `useFieldArray` gerencia o array de itens:**

```
Estado inicial: fields = [{ id: 'rhf-0', produtoId: '', quantidade: 1 }]

Usuario clica "Adicionar item":
  append({ produtoId: '', quantidade: 1 })
  fields = [
    { id: 'rhf-0', produtoId: '', quantidade: 1 },
    { id: 'rhf-1', produtoId: '', quantidade: 1 },  <- novo
  ]

Zod valida o array completo:
  itens.min(1) -> garante pelo menos 1 item
  itens[n].produtoId.min(1) -> garante que produto foi selecionado
  itens[n].quantidade.min(1) -> garante quantidade valida
```

**Por que `watch('itens')` e nao `getValues('itens')`?**  
`watch` e reativo - retorna o valor atualizado a cada renderizacao, o que permite que
`calcularTotal()` recalcule o total toda vez que o usuario muda um select ou quantidade.
`getValues` e uma leitura pontual e nao causa re-renderizacao.

---

## 12. Dashboard - DashboardPage.tsx

```
src/pages/dashboard/DashboardPage.tsx
```

**O que esta pagina faz:** Reutiliza o cache de produtos para derivar metricas do
catalogo e exibe cartoes de navegacao rapida para as secoes de Produtos e Pedidos.

```tsx
export function DashboardPage() {
  const navigate = useNavigate();

  // Reutiliza o mesmo hook de produtos usado em ProdutosPage
  // Se o usuario veio de ProdutosPage, os dados ja estao no cache - zero requisicoes!
  const { data: produtos, isLoading } = useProdutos();

  if (isLoading) return <PageSpinner />;

  // Derivacoes calculadas no cliente - sem endpoint especifico de dashboard
  const totalProdutos = produtos?.length ?? 0;
  const semEstoque = produtos?.filter((p) => p.estoque === 0).length ?? 0;
  const estoqueTotal = produtos?.reduce((acc, p) => acc + p.estoque, 0) ?? 0;
  const valorCatalogo = produtos?.reduce((acc, p) => acc + p.preco * p.estoque, 0) ?? 0;

  return (
    <div>
      {/* Grid de metricas */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardContent>
            <p>Total de Produtos</p>
            <p>{totalProdutos}</p>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <p>Sem Estoque</p>
            <p>{semEstoque}</p>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <p>Unidades em Estoque</p>
            <p>{estoqueTotal}</p>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <p>Valor do Catalogo</p>
            <p>{formatCurrency(valorCatalogo)}</p>
          </CardContent>
        </Card>
      </div>

      {/* Cartoes de navegacao rapida */}
      <div className="grid gap-4 sm:grid-cols-2">
        <Card>
          <CardContent>
            <p>Produtos</p>
            <p>Gerencie o catalogo de produtos</p>
            <Button onClick={() => navigate({ to: '/produtos' })}>
              Acessar Produtos
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <p>Pedidos</p>
            <p>Consulte e crie pedidos de clientes</p>
            <Button onClick={() => navigate({ to: '/pedidos' })}>
              Acessar Pedidos
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
```

**Ponto chave - compartilhamento de cache:**

```
Usuario acessa /produtos (ProdutosPage)
  -> useProdutos() -> query ['produtos'] -> requisicao GET -> cache preenchido

Usuario navega para / (DashboardPage)
  -> useProdutos() -> query ['produtos'] -> cache ainda valido (staleTime 30s)
  -> ZERO nova requisicao - os dados ja estao la

Usuario cria um produto em /produtos/novo
  -> invalidateQueries(['produtos']) -> cache expirado
  -> DashboardPage re-busca automaticamente e exibe metricas atualizadas
```

As metricas sao **derivadas no cliente** - nao existe endpoint `/api/dashboard` no
backend. O frontend ja possui os dados e os transforma com `.filter()`, `.reduce()` e
`formatCurrency()`.

---

## 13. Fluxo Completo de Dados - Do Clique a API

### 13.1 Criar Produto

```
USUARIO clica em "Criar Produto"
  |
  v
handleSubmit() (React Hook Form)
  -> coleta: { nome, descricao, preco, estoque }
  |
  v
zodResolver -> schema.parse(values)
  -- FALHA -> errors exibidos nos campos -> formulario nao submete
  -- SUCESSO -> onSubmit(data)
        |
        v
      criar.mutate(data)  [useCriarProduto]
        -> isPending = true -> Button mostra spinner
        |
        v
      produtosService.criar(data)
        |
        v
      produtosApi.post('/', data)  [Axios]
        -> serializa para JSON
        -> header: Content-Type: application/json
        |
        v
      Vite Proxy -> POST http://localhost:5001/api/produtos/
        |
        v
      API .NET - Produtos (porta 5001)
        -> ProdutosEndpoints -> CriarProdutoCommandHandler
        -> FluentValidation -> Produto.Criar() -> SaveChanges()
        -> Retorna 201 + { id, nome, preco, ... }
        |
        v
      onSuccess()
        -> invalidateQueries(['produtos']) -> cache da lista expirado
        -> navigate({ to: '/produtos' })
        |
        v
      ProdutosPage monta -> useProdutos() re-busca -> lista atualizada
```

### 13.2 Criar Pedido

```
USUARIO clica em "Criar Pedido"
  |
  v
handleSubmit() (React Hook Form)
  -> coleta: { clienteId, itens: [{ produtoId, quantidade }, ...] }
  |
  v
zodResolver -> schema.parse(values)
  -- FALHA -> errors.clienteId / errors.itens[n] exibidos
  -- SUCESSO -> onSubmit(data)
        |
        v
      criar.mutate(data)  [useCriarPedido]
        |
        v
      pedidosService.criar(data)
        |
        v
      pedidosApi.post('/', data)  [Axios]
        |
        v
      Vite Proxy -> POST http://localhost:5002/api/pedidos/
        |
        v
      API .NET - Pedidos (porta 5002)
        -> PedidosEndpoints -> CriarPedidoCommandHandler
        -> Chama API de Produtos (HTTP sincrono) -> valida estoque
        -> Pedido.Criar() -> SaveChanges()
        -> Publica PedidoConfirmadoEvent -> RabbitMQ -> Produtos debita estoque
        -> Retorna 201 + { pedidoId: "novo-guid" }
        |
        v
      onSuccess(res)
        -> setPedidoId(res.pedidoId) -> tela de sucesso exibida inline
        -> invalidateQueries(['pedidos', 'cliente', clienteId])

      Tela de sucesso exibe o pedidoId
      Botao "Ver Pedidos" -> navigate({ to: '/pedidos' })
```

**Diferenca arquitetural entre os dois fluxos:**
- Criar Produto: chamada direta - 1 servico, 1 banco de dados
- Criar Pedido: envolve validacao cross-service (Pedidos chama Produtos via HTTP) +
  evento assincrono (RabbitMQ) para debitar estoque

---

## 14. Utilitarios - lib/utils.ts

```
src/lib/utils.ts
```

```typescript
// Combina classes Tailwind com suporte a condicionais e resolucao de conflitos
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
// cn('p-4', condition && 'bg-red-500') -> 'p-4 bg-red-500' ou 'p-4'
// cn('p-4', 'p-8') -> 'p-8'   (twMerge resolve o conflito, ultima vence)

// Formata numero para moeda brasileira usando a API nativa do browser
export function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value);
}
// formatCurrency(99.9)    -> "R$ 99,90"
// formatCurrency(1234.5)  -> "R$ 1.234,50"

// Formata string ISO 8601 para data/hora legivel em pt-BR
export function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(dateString));
}
// formatDate("2026-05-16T04:16:16.661") -> "16/05/2026, 01:16"
```

`Intl.NumberFormat` e `Intl.DateTimeFormat` sao APIs nativas do browser - sem
biblioteca externa - e respeitam as convencoes de localidade (virgula decimal, ponto de
milhar, formato de data).

**Onde cada utilitario e usado:**

| Funcao | Usado em |
|--------|----------|
| `cn()` | Todos os componentes UI (Button, Input, Card, Badge, Navbar, ...) |
| `formatCurrency()` | ProdutosPage, PedidosPage, NovoPedidoPage, DashboardPage |
| `formatDate()` | ProdutosPage (criadoEm), PedidosPage (criadoEm do pedido) |

---

## 15. Diagrama de Dependencias entre Camadas

```
main.tsx
  +-- router.ts
  |     +-- RootLayout -> Navbar, Outlet
  |
  +-- QueryClient (TanStack Query)
  |
  +-- PAGINAS
        |
        +-- DashboardPage.tsx
        |     +-- useProdutos()              -> produtosService.listar()
        |
        +-- ProdutosPage.tsx
        |     +-- useProdutos()              -> produtosService.listar()
        |     +-- useExcluirProduto()        -> produtosService.excluir(id)
        |
        +-- NovoProdutoPage.tsx
        |     +-- useCriarProduto()          -> produtosService.criar(payload)
        |
        +-- EditarProdutoPage.tsx
        |     +-- useProduto(id)             -> produtosService.obterPorId(id)
        |     +-- useAtualizarProduto()      -> produtosService.atualizar(id, payload)
        |
        +-- PedidosPage.tsx
        |     +-- usePedidosPorCliente(id)   -> pedidosService.listarPorCliente(id)
        |
        +-- NovoPedidoPage.tsx
              +-- useProdutos()              -> produtosService.listar() (para o select)
              +-- useCriarPedido()           -> pedidosService.criar(payload)

HOOKS
  +-- useProdutos.ts  -> produtosService  (src/services/produtosService.ts)
  +-- usePedidos.ts   -> pedidosService   (src/services/pedidosService.ts)

SERVICOS
  +-- produtosService.ts -> produtosApi  (baseURL: '/api/produtos')
  +-- pedidosService.ts  -> pedidosApi   (baseURL: '/api/pedidos')

HTTP CLIENT (src/lib/http.ts)
  +-- produtosApi -> axios.create({ baseURL: '/api/produtos' })
  +-- pedidosApi  -> axios.create({ baseURL: '/api/pedidos' })

PROXY VITE (vite.config.ts)
  +-- /api/produtos -> http://localhost:5001  (Produtos API)
  +-- /api/pedidos  -> http://localhost:5002  (Pedidos API)

TIPOS
  +-- src/types/produto.ts -> Produto, CriarProdutoRequest, AtualizarProdutoRequest
  +-- src/types/pedido.ts  -> Pedido, CriarPedidoRequest, PedidoCriado,
                              ItemPedido, ItemPedidoDetalhe, StatusPedidoNumerico

COMPONENTES UI COMPARTILHADOS (src/components/ui/)
  +-- Button       -> todas as acoes de navegacao, submit e exclusao
  +-- Input        -> todos os formularios (Produtos e Pedidos) + busca em PedidosPage
  +-- Card / CardHeader / CardTitle / CardContent
  |                -> Dashboard, PedidosPage, NovoPedidoPage, NovoProdutoPage
  +-- Badge        -> estoque em ProdutosPage, status do pedido em PedidosPage
  +-- EmptyState   -> estados vazios em ProdutosPage e PedidosPage
  +-- Spinner / PageSpinner -> carregamento em todas as paginas

UTILITARIOS (src/lib/utils.ts)
  +-- cn()             -> todos os componentes UI
  +-- formatCurrency() -> Produtos, Pedidos, Dashboard
  +-- formatDate()     -> Produtos, Pedidos
```

---

*Documentacao gerada para o projeto `arquitetura.client`. Para a documentacao completa
do backend e da infraestrutura, consulte `Arquitetura.Server/BACKEND.md` e o
`README.md` na raiz da solucao.*
