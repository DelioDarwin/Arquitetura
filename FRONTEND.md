# Documentação do Frontend — Arquitetura.Frontend

> Guia de estudo detalhado que explica cada camada da interface, do clique do usuário
> até a chamada HTTP à API e o retorno dos dados na tela. Cobre os dois CRUDs da
> aplicação — **Produtos** e **Pedidos** — a consulta de **CEP** e destaca os
> componentes e padrões compartilhados entre eles.

---

## Sumário

1. [Visão Geral da Arquitetura Frontend](#1-visão-geral-da-arquitetura-frontend)
2. [Ponto de Entrada — main.tsx](#2-ponto-de-entrada--maintsx)
3. [Roteamento — router.ts](#3-roteamento--routerts)
4. [Shell da Aplicação — Layout e Navbar](#4-shell-da-aplicação--layout-e-navbar)
5. [Tipos TypeScript — O Contrato com a API](#5-tipos-typescript--o-contrato-com-a-api)
   - 5.1 [Tipos de Produto](#51-tipos-de-produto)
   - 5.2 [Tipos de Pedido](#52-tipos-de-pedido)
   - 5.3 [Tipos de CEP](#53-tipos-de-cep)
6. [Camada HTTP — lib/http.ts](#6-camada-http--libhttpts)
7. [Camadas de Serviço](#7-camadas-de-serviço)
   - 7.1 [produtosService.ts](#71-produtosservicets)
   - 7.2 [pedidosService.ts](#72-pedidosservicets)
   - 7.3 [cepService.ts](#73-cepservicets)
8. [Camadas de Hooks](#8-camadas-de-hooks)
   - 8.1 [useProdutos.ts](#81-useprodutosts)
   - 8.2 [usePedidos.ts](#82-usepedidosts)
   - 8.3 [useCep.ts](#83-usecepts)
9. [Componentes UI Compartilhados](#9-componentes-ui-compartilhados)
   - 9.1 [Button](#91-button)
   - 9.2 [Input](#92-input)
   - 9.3 [Card, CardHeader, CardTitle, CardContent](#93-card-cardheader-cardtitle-cardcontent)
   - 9.4 [Badge](#94-badge)
   - 9.5 [EmptyState](#95-emptystate)
   - 9.6 [Spinner e PageSpinner](#96-spinner-e-pagespinner)
   - 9.7 [cn() — Classes condicionais](#97-cn--classes-condicionais)
10. [CRUD de Produtos — Página a Página](#10-crud-de-produtos--página-a-página)
    - 10.1 [Listar Produtos — ProdutosPage.tsx](#101-listar-produtos--produtospagetsx)
    - 10.2 [Criar Produto — NovoProdutoPage.tsx](#102-criar-produto--novoprodutopagetsx)
    - 10.3 [Editar Produto — EditarProdutoPage.tsx](#103-editar-produto--editarprodutopagetsx)
    - 10.4 [Excluir Produto — Fluxo inline na Lista](#104-excluir-produto--fluxo-inline-na-lista)
11. [CRUD de Pedidos — Página a Página](#11-crud-de-pedidos--página-a-página)
    - 11.1 [Listar Pedidos — PedidosPage.tsx](#111-listar-pedidos--pedidospagetsx)
    - 11.2 [Criar Pedido — NovoPedidoPage.tsx](#112-criar-pedido--novopedidopagetsx)
12. [Consulta de CEP — ConsultaCepPage.tsx](#12-consulta-de-cep--consultaceppagetsx)
13. [Dashboard — DashboardPage.tsx](#13-dashboard--dashboardpagetsx)
14. [Fluxo Completo de Dados — Do Clique à API](#14-fluxo-completo-de-dados--do-clique-à-api)
    - 14.1 [Criar Produto](#141-criar-produto)
    - 14.2 [Criar Pedido](#142-criar-pedido)
    - 14.3 [Consultar CEP](#143-consultar-cep)
15. [Utilitários — lib/utils.ts](#15-utilitários--libutilsts)
16. [Diagrama de Dependências entre Camadas](#16-diagrama-de-dependências-entre-camadas)

---

## 1. Visão Geral da Arquitetura Frontend

O frontend segue uma arquitetura em **cinco camadas horizontais**. Cada camada tem uma
responsabilidade única e só conhece a camada imediatamente abaixo dela:

```
┌───────────────────────────────────────────────────────┐
│  PÁGINAS  (src/pages/)                                │
│  Composição completa de uma tela. Usa hooks,          │
│  componentes UI e navegação.                          │
├───────────────────────────────────────────────────────┤
│  HOOKS  (src/hooks/)                                  │
│  Estado de servidor com TanStack Query.               │
│  Cache, loading, erro, mutações e invalidação.        │
├───────────────────────────────────────────────────────┤
│  SERVIÇOS  (src/services/)                            │
│  Funções async puras que chamam Axios.                │
│  Não conhecem React — podem ser testadas sozinhas.    │
├───────────────────────────────────────────────────────┤
│  HTTP CLIENT  (src/lib/http.ts)                       │
│  Instâncias Axios com baseURL e headers padrão.       │
│  O Proxy do Vite redireciona para as APIs reais.      │
├───────────────────────────────────────────────────────┤
│  TIPOS  (src/types/)                                  │
│  Interfaces TypeScript que espelham os DTOs da API.   │
│  Garantem que o contrato com o backend é respeitado.  │
└───────────────────────────────────────────────────────┘
```

**Componentes UI** (`src/components/`) são uma camada transversal — elementos visuais
reutilizáveis sem lógica de negócio, usados em qualquer página.

---

## 2. Ponto de Entrada — main.tsx

```
src/main.tsx
```

Este é o arquivo que o Vite executa primeiro. Ele monta toda a árvore de providers que
envolve a aplicação.

```tsx
// src/main.tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from '@tanstack/react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { router } from './router';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 30, // dados ficam "frescos" por 30 segundos
      retry: 1,             // tenta novamente 1x em caso de erro de rede
    },
  },
});

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
| `StrictMode` | Detecta efeitos colaterais e APIs depreciadas no desenvolvimento. Sem efeito em produção. |
| `QueryClientProvider` | Injeta o `queryClient` via Context, tornando o cache do TanStack Query acessível em qualquer componente filho via hooks. |
| `RouterProvider` | Observa a URL do browser e renderiza o componente de página correspondente à rota atual. |
| `ReactQueryDevtools` | Painel flutuante apenas em `npm run dev`. Exibe estado do cache, queries ativas e histórico de mutações. Removido automaticamente no build de produção. |

---

## 3. Roteamento — router.ts

```
src/router.ts
```

O TanStack Router usa uma árvore de rotas construída manualmente em código. A aplicação
possui **7 rotas** registradas:

```typescript
// src/router.ts

const rootRoute = createRootRoute({ component: RootLayout });

const indexRoute        = createRoute({ getParentRoute: () => rootRoute, path: '/',                        component: DashboardPage });
const produtosRoute     = createRoute({ getParentRoute: () => rootRoute, path: '/produtos',                component: ProdutosPage });
const novoProdutoRoute  = createRoute({ getParentRoute: () => rootRoute, path: '/produtos/novo',           component: NovoProdutoPage });
const editarProdutoRoute= createRoute({ getParentRoute: () => rootRoute, path: '/produtos/$id/editar',     component: EditarProdutoPage });
const pedidosRoute      = createRoute({ getParentRoute: () => rootRoute, path: '/pedidos',                 component: PedidosPage });
const novoPedidoRoute   = createRoute({ getParentRoute: () => rootRoute, path: '/pedidos/novo',            component: NovoPedidoPage });
const cepRoute          = createRoute({ getParentRoute: () => rootRoute, path: '/cep',                     component: ConsultaCepPage });

const routeTree = rootRoute.addChildren([
  indexRoute, produtosRoute, novoProdutoRoute, editarProdutoRoute,
  pedidosRoute, novoPedidoRoute, cepRoute,
]);

export const router = createRouter({ routeTree });

declare module '@tanstack/react-router' {
  interface Register { router: typeof router; }
}
```

### Mapa de rotas

| URL | Componente | Descrição |
|-----|-----------|-----------|
| `/` | `DashboardPage` | Métricas e navegação rápida |
| `/produtos` | `ProdutosPage` | Listagem + excluir inline |
| `/produtos/novo` | `NovoProdutoPage` | Formulário de criação |
| `/produtos/$id/editar` | `EditarProdutoPage` | Formulário de edição |
| `/pedidos` | `PedidosPage` | Busca por cliente + listagem |
| `/pedidos/novo` | `NovoPedidoPage` | Formulário de criação com itens |
| `/cep` | `ConsultaCepPage` | Consulta de endereço por CEP |

### Navegação programática

```typescript
const navigate = useNavigate();

navigate({ to: '/produtos' });
navigate({ to: '/produtos/$id/editar', params: { id } }); // rota com parâmetro tipado
```

### Leitura de parâmetros de rota

```typescript
// Na página EditarProdutoPage:
const { id } = useParams({ strict: false }) as { id: string };
// id === "abc-123" quando a URL for /produtos/abc-123/editar
```

---

## 4. Shell da Aplicação — Layout e Navbar

```
src/components/layout/RootLayout.tsx
src/components/layout/Navbar.tsx
```

### RootLayout

```tsx
export function RootLayout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <Outlet /> {/* página da rota atual é injetada aqui */}
      </main>
    </div>
  );
}
```

`<Outlet />` é o "buraco" onde o TanStack Router injeta o componente da rota ativa.

### Navbar

A Navbar possui **quatro itens** de navegação, incluindo a rota de CEP:

```tsx
const navItems = [
  { to: '/',        label: 'Dashboard', icon: LayoutDashboard },
  { to: '/produtos', label: 'Produtos',  icon: Package },
  { to: '/pedidos',  label: 'Pedidos',   icon: ShoppingCart },
  { to: '/cep',      label: 'CEP',       icon: MapPin },
];

export function Navbar() {
  const { location } = useRouterState();

  return (
    <header className="sticky top-0 z-40 ...">
      {navItems.map(({ to, label, icon: Icon }) => {
        const isActive =
          to === '/'
            ? location.pathname === '/'
            : location.pathname.startsWith(to);

        return (
          <Link key={to} to={to}
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
- `startsWith(to)` faz com que `/produtos/novo` e `/produtos/abc/editar` mantenham "Produtos" ativo
- `<Link>` gera um `<a>` que navega sem recarregar a página (SPA)

---

## 5. Tipos TypeScript — O Contrato com a API

Os tipos espelham exatamente os DTOs retornados pelas APIs .NET. Cada domínio tem seu
próprio arquivo de tipos.

### 5.1 Tipos de Produto

```typescript
// src/types/produto.ts

export interface Produto {
  id: string;           // Guid do backend → string no TypeScript
  nome: string;
  descricao: string;
  preco: number;        // decimal → number
  estoque: number;      // int → number
  criadoEm: string;     // DateTime → string ISO 8601
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
```

### 5.2 Tipos de Pedido

```typescript
// src/types/pedido.ts

export interface ItemPedido {
  produtoId: string;
  quantidade: number;
}

export interface CriarPedidoRequest {
  clienteId: string;
  itens: ItemPedido[];
}

export interface ItemPedidoDetalhe {
  produtoId: string;
  nomeProduto: string;
  quantidade: number;
  precoUnitario: number;
  subtotal: number;
}

// O backend serializa o enum StatusPedido como número por padrão
export type StatusPedidoNumerico = 0 | 1 | 2;

export interface Pedido {
  id: string;
  clienteId?: string;
  status: StatusPedidoNumerico;
  total: number;
  itens?: ItemPedidoDetalhe[];
  criadoEm: string;
  atualizadoEm?: string;
}

export interface PedidoCriado {
  pedidoId: string;  // apenas o id — sem o objeto completo
}
```

**Mapeamento de `StatusPedidoNumerico` para labels:**

```typescript
const statusLabel: Record<StatusPedidoNumerico, string> = {
  0: 'Pendente', 1: 'Confirmado', 2: 'Cancelado',
};

const statusVariant: Record<StatusPedidoNumerico, 'warning' | 'success' | 'destructive'> = {
  0: 'warning', 1: 'success', 2: 'destructive',
};
```

### 5.3 Tipos de CEP

```typescript
// src/types/cep.ts

export interface ViaCep {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  localidade: string;
  uf: string;
  ibge: string;
  gia: string;
  ddd: string;
  siafi: string;
}
```

O `ViaCep` espelha o `ViaCepDto` retornado pela Pedidos API, que por sua vez espelha a
resposta da API pública ViaCEP. O frontend nunca chama `viacep.com.br` diretamente —
passa pela API de Pedidos.

---

## 6. Camada HTTP — lib/http.ts

```
src/lib/http.ts
```

Esta camada cria as instâncias do Axios pré-configuradas. **Nenhuma outra parte do
código importa o Axios diretamente** — sempre usam estas instâncias.

```typescript
// src/lib/http.ts
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

// Instância dedicada para consulta de CEP (roteada pela Pedidos API)
export const cepApi = axios.create({
  baseURL: '/api/cep',
  headers: { 'Content-Type': 'application/json' },
});

export function extractApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error) && error.response?.data) {
    return error.response.data as ApiError;
  }
  return { title: 'Erro inesperado. Tente novamente.', status: 500 };
}
```

### O Proxy do Vite

Durante o desenvolvimento, o Vite intercepta requisições e as encaminha para as APIs reais:

```
Browser (5173)          Vite Dev Server            API .NET
     |                        |                        |
     |-- GET /api/produtos/ -->|                        |
     |                        |-- GET :5001/api/... -->|
     |                        |<-- 200 [produtos] -----|
     |<-- 200 [produtos] ------|                        |
     |                        |                        |
     |-- GET /api/cep/01310 -->|                        |
     |                        |-- GET :5002/api/cep -->|
     |                        |<-- 200 [endereço] -----|
     |<-- 200 [endereço] ------|                        |
```

**Por que usar proxy?** Sem ele, o browser bloquearia as requisições por **CORS**
(Cross-Origin Resource Sharing) — cliente na porta 5173, APIs nas portas 5001/5002.

### Configuração do proxy em vite.config.ts

```typescript
server: {
  proxy: {
    '/api/produtos': { target: 'http://localhost:5001', changeOrigin: true },
    '/api/pedidos':  { target: 'http://localhost:5002', changeOrigin: true },
    '/api/cep':      { target: 'http://localhost:5002', changeOrigin: true },
  }
}
```

**Detalhe importante:** `/api/cep` é roteado para a **Pedidos API** (porta 5002),
pois é o serviço de Pedidos que integra com o ViaCEP e expõe o endpoint de CEP.

---

## 7. Camadas de Serviço

Os serviços são **funções async puras**. Não conhecem React, não usam hooks e não
gerenciam estado — apenas fazem chamadas HTTP e retornam dados tipados.

### 7.1 produtosService.ts

```typescript
// src/services/produtosService.ts

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
    await produtosApi.put(`/${id}`, payload);  // 204 No Content
  },

  async excluir(id: string): Promise<void> {
    await produtosApi.delete(`/${id}`);  // 204 No Content
  },
};
```

| Método | URL enviada ao Vite | URL que chega na API |
|--------|--------------------|--------------------|
| `listar()` | `GET /api/produtos/` | `GET :5001/api/produtos/` |
| `obterPorId(id)` | `GET /api/produtos/{id}` | `GET :5001/api/produtos/{id}` |
| `criar(payload)` | `POST /api/produtos/` | `POST :5001/api/produtos/` |
| `atualizar(id, payload)` | `PUT /api/produtos/{id}` | `PUT :5001/api/produtos/{id}` |
| `excluir(id)` | `DELETE /api/produtos/{id}` | `DELETE :5001/api/produtos/{id}` |

### 7.2 pedidosService.ts

```typescript
// src/services/pedidosService.ts

export const pedidosService = {
  async listarPorCliente(clienteId: string): Promise<Pedido[]> {
    const { data } = await pedidosApi.get<Pedido[]>(`/cliente/${clienteId}`);
    return data;
  },

  async obterPorId(id: string): Promise<Pedido> {
    const { data } = await pedidosApi.get<Pedido>(`/${id}`);
    return data;
  },

  async criar(payload: CriarPedidoRequest): Promise<PedidoCriado> {
    const { data } = await pedidosApi.post<PedidoCriado>('/', payload);
    return data;  // { pedidoId: "novo-guid" }
  },
};
```

### 7.3 cepService.ts

```typescript
// src/services/cepService.ts

export const cepService = {
  consultar: async (cep: string): Promise<ViaCep> => {
    const { data } = await cepApi.get<ViaCep>(`/${cep}`);
    return data;
  },
};
```

| Método | URL enviada ao Vite | URL que chega na API |
|--------|--------------------|--------------------|
| `consultar(cep)` | `GET /api/cep/{cep}` | `GET :5002/api/cep/{cep}` |

O `cepService` é a camada mais simples do projeto — uma única operação de leitura. Toda a complexidade de integração com o ViaCEP fica encapsulada no backend (Pedidos API).

---

## 8. Camadas de Hooks

Os hooks conectam o React ao TanStack Query, adicionando **cache**, **estados de
carregamento**, **tratamento de erro** e **invalidação automática** sobre os serviços.

### 8.1 useProdutos.ts

```typescript
// src/hooks/useProdutos.ts

export const produtosKeys = {
  all: ['produtos'] as const,
  detail: (id: string) => ['produtos', id] as const,
};

export function useProdutos() {
  return useQuery({ queryKey: produtosKeys.all, queryFn: produtosService.listar });
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
```

### Ciclo de vida do cache

```
Componente monta → useQuery é chamado
        ↓
Cache tem dados?
   -- NÃO → isLoading = true → queryFn() → dados chegam → renderiza
   -- SIM → renderiza imediatamente com dados do cache
                 ↓
          Dados estão stale? (>30s ou invalidados)
          -- NÃO → não faz nova requisição
          -- SIM → isFetching = true (re-busca em background)
                        ↓
                   Dados chegam → atualiza cache → componente re-renderiza
```

### 8.2 usePedidos.ts

```typescript
// src/hooks/usePedidos.ts

export const pedidosKeys = {
  cliente: (clienteId: string) => ['pedidos', 'cliente', clienteId] as const,
  detail: (id: string) => ['pedidos', id] as const,
};

export function usePedidosPorCliente(clienteId: string) {
  return useQuery({
    queryKey: pedidosKeys.cliente(clienteId),
    queryFn: () => pedidosService.listarPorCliente(clienteId),
    enabled: !!clienteId,
  });
}

export function usePedido(id: string) {
  return useQuery({
    queryKey: pedidosKeys.detail(id),
    queryFn: () => pedidosService.obterPorId(id),
    enabled: !!id,
  });
}

export function useCriarPedido() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarPedidoRequest) => pedidosService.criar(payload),
    onSuccess: (_data, vars) => {
      queryClient.invalidateQueries({ queryKey: pedidosKeys.cliente(vars.clienteId) });
    },
  });
}
```

### 8.3 useCep.ts

```typescript
// src/hooks/useCep.ts

export const cepKeys = {
  detail: (cep: string) => ['cep', cep] as const,
};

export function useCep(cep: string) {
  return useQuery({
    queryKey: cepKeys.detail(cep),
    queryFn: () => cepService.consultar(cep),
    enabled: cep.replace(/\D/g, '').length === 8, // só executa com 8 dígitos
    retry: false,                                  // CEP inválido não deve ser retentado
    staleTime: 1000 * 60 * 60,                     // 1 hora — CEP não muda
  });
}
```

**Por que `staleTime` de 1 hora?**
Endereços de CEP são estáticos — não mudam entre requisições. Cachear por 1 hora evita
chamadas desnecessárias ao backend quando o usuário consulta o mesmo CEP repetidamente.

**Por que `retry: false`?**
Se o CEP não foi encontrado (404) ou é inválido, tentar novamente é inútil. O erro deve
ser exibido imediatamente para o usuário corrigir a entrada.

**Comparação dos três hooks:**

| Aspecto | useProdutos | usePedidos | useCep |
|---------|-------------|------------|--------|
| Tipo de operação | Query + Mutação | Query + Mutação | Somente Query |
| Chave de cache | `['produtos']` | `['pedidos', 'cliente', id]` | `['cep', cep]` |
| `staleTime` | 30s (padrão) | 30s (padrão) | 1 hora |
| `retry` | 1x (padrão) | 1x (padrão) | `false` |
| `enabled` | sempre | `!!clienteId` | 8 dígitos numéricos |

---

## 9. Componentes UI Compartilhados

```
src/components/ui/
```

Todos os componentes UI são **completamente independentes de domínio** — não sabem se
estão numa tela de Produtos, Pedidos ou CEP.

### 9.1 Button

```tsx
const variants = {
  primary:     'bg-indigo-600 text-white hover:bg-indigo-700 ...',
  secondary:   'bg-gray-100 text-gray-900 hover:bg-gray-200 ...',
  destructive: 'bg-red-600 text-white hover:bg-red-700 ...',
  ghost:       'text-gray-700 hover:bg-gray-100 ...',
  outline:     'border border-gray-300 text-gray-700 hover:bg-gray-50 ...',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, disabled, children, className, ...props }, ref) => (
    <button
      ref={ref}
      disabled={disabled || loading}
      className={cn('inline-flex items-center justify-center ...', variants[variant], sizes[size], ...)}
      {...props}
    >
      {loading && <svg className="animate-spin h-4 w-4">...</svg>}
      {children}
    </button>
  )
);
```

**Onde é usado:**
- `variant="primary"` — Novo Produto, Novo Pedido, Criar/Salvar em formulários, Buscar CEP
- `variant="secondary"` — botão Buscar em PedidosPage
- `variant="destructive"` — confirmação de exclusão
- `variant="ghost"` — botão voltar em NovoPedidoPage
- `variant="outline"` — "Adicionar item" em NovoPedidoPage
- `loading={isPending}` — qualquer botão de submit aguardando resposta da API

### 9.2 Input

```tsx
export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className, id, ...props }, ref) => (
    <div className="flex flex-col gap-1">
      {label && <label htmlFor={id} className="...">{label}</label>}
      <input
        ref={ref}  // forwardRef → React Hook Form pode registrar o input
        id={id}
        className={cn('h-9 w-full rounded-md border ...', error && 'border-red-500', className)}
        {...props}
      />
      {error && <p className="text-xs text-red-600">{error}</p>}
    </div>
  )
);
```

**Por que `forwardRef` é essencial aqui:** O `register('campo')` do React Hook Form
retorna `{ ref, name, onChange, onBlur }`. A `ref` só chega ao `<input>` nativo se o
componente usar `forwardRef`.

### 9.3 Card, CardHeader, CardTitle, CardContent

```tsx
export function Card({ children, className }: CardProps) {
  return <div className={cn('rounded-xl border border-gray-200 bg-white shadow-sm', className)}>{children}</div>;
}

export function CardHeader({ children, className }: CardProps) {
  return <div className={cn('flex items-center justify-between px-6 py-4 border-b', className)}>{children}</div>;
}

export function CardTitle({ children, className }: CardProps) {
  return <h3 className={cn('text-base font-semibold text-gray-900', className)}>{children}</h3>;
}

export function CardContent({ children, className }: CardProps) {
  return <div className={cn('px-6 py-4', className)}>{children}</div>;
}
```

**Padrão de composição:**
```tsx
<Card>
  <CardHeader>
    <CardTitle>Itens do Pedido</CardTitle>
    <Button variant="outline" size="sm">Adicionar item</Button>
  </CardHeader>
  <CardContent>...</CardContent>
</Card>
```

### 9.4 Badge

```tsx
const variants = {
  success:     'bg-emerald-100 text-emerald-700',  // estoque > 0, pedido Confirmado
  warning:     'bg-amber-100 text-amber-700',       // pedido Pendente
  destructive: 'bg-red-100 text-red-700',           // sem estoque, pedido Cancelado
  outline:     'border border-gray-300 text-gray-600',
};

export function Badge({ variant = 'default', children, className }: BadgeProps) { ... }
```

**Onde é usado:**
- `ProdutosPage` — estoque disponível (`success`) ou esgotado (`destructive`)
- `PedidosPage` — status do pedido: Pendente (`warning`), Confirmado (`success`), Cancelado (`destructive`)

### 9.5 EmptyState

```tsx
interface EmptyStateProps {
  title: string;
  description?: string;
  action?: React.ReactNode;
}
```

**Onde é usado:**
- `ProdutosPage` — quando não há produtos cadastrados
- `PedidosPage` — quando nenhum `clienteId` foi informado e quando a busca não retorna pedidos

### 9.6 Spinner e PageSpinner

```tsx
// Spinner inline — dentro de botões e pequenas áreas
export function Spinner({ className }: { className?: string }) { ... }

// PageSpinner — conteúdo principal enquanto a página carrega
export function PageSpinner() {
  return (
    <div className="flex h-64 items-center justify-center">
      <Spinner className="h-8 w-8" />
    </div>
  );
}
```

**Onde é usado:**
- `PageSpinner` — retornado diretamente em `ProdutosPage`, `EditarProdutoPage`, `DashboardPage` e `PedidosPage` durante `isLoading = true`
- `Spinner` embutido no `Button` — exibido automaticamente quando `loading={true}`
- `ConsultaCepPage` — exibido dentro do card de resultado enquanto `isFetching = true`

### 9.7 cn() — Classes condicionais

```typescript
// src/lib/utils.ts
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

cn('base', condition && 'extra', false && 'ignored') // → 'base extra'
cn('p-4', 'p-8')                                    // → 'p-8' (última vence)
cn('text-red-500', 'text-blue-600')                 // → 'text-blue-600'
```

---

## 10. CRUD de Produtos — Página a Página

### 10.1 Listar Produtos — ProdutosPage.tsx

```
src/pages/produtos/ProdutosPage.tsx
```

**O que faz:** Busca todos os produtos da API, renderiza cards com nome, preço, estoque
e ações de editar/excluir.

```tsx
export function ProdutosPage() {
  const navigate = useNavigate();
  const { data: produtos, isLoading, isError } = useProdutos();
  const excluir = useExcluirProduto();
  const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

  if (isLoading) return <PageSpinner />;
  if (isError) return <p>Erro ao carregar produtos.</p>;

  return (
    <div>
      <Button onClick={() => navigate({ to: '/produtos/novo' })}>Novo Produto</Button>

      {!produtos?.length ? (
        <EmptyState title="Nenhum produto" />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {produtos.map((produto) => (
            <Card key={produto.id}>
              <Badge variant={produto.estoque > 0 ? 'success' : 'destructive'}>
                {produto.estoque > 0 ? `${produto.estoque} un.` : 'Sem estoque'}
              </Badge>

              <Button onClick={() => navigate({ to: '/produtos/$id/editar', params: { id: produto.id } })}>
                Editar
              </Button>

              {confirmDelete === produto.id ? (
                <Button variant="destructive" loading={excluir.isPending}
                  onClick={() => excluir.mutate(produto.id, { onSuccess: () => setConfirmDelete(null) })}>
                  Confirmar
                </Button>
              ) : (
                <Button onClick={() => setConfirmDelete(produto.id)}><Trash2 /></Button>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
```

### 10.2 Criar Produto — NovoProdutoPage.tsx

```
src/pages/produtos/NovoProdutoPage.tsx
```

```tsx
const schema = z.object({
  nome:      z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  descricao: z.string().min(5, 'Descrição deve ter pelo menos 5 caracteres'),
  preco:     z.number().positive('Preço deve ser positivo'),
  estoque:   z.number().int().min(0, 'Estoque não pode ser negativo'),
});

export function NovoProdutoPage() {
  const navigate = useNavigate();
  const criar = useCriarProduto();
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) =>
    criar.mutate(data, { onSuccess: () => navigate({ to: '/produtos' }) });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input id="nome" label="Nome *" error={errors.nome?.message} {...register('nome')} />
      <Input id="preco" type="number" step="0.01" error={errors.preco?.message}
             {...register('preco', { valueAsNumber: true })} />
      <Button type="submit" loading={criar.isPending}>Criar Produto</Button>
    </form>
  );
}
```

**Fluxo de validação:**
```
Clique em "Criar Produto"
  → handleSubmit() → zodResolver.parse(values)
     ├── FALHA → errors exibidos nos campos
     └── SUCESSO → onSubmit(data)
                      → criar.mutate(data)
                         → produtosService.criar()
                         → POST /api/produtos/
                            ├── 201 → invalidate cache → navigate('/produtos')
                            └── 4xx → isError = true → mensagem exibida
```

### 10.3 Editar Produto — EditarProdutoPage.tsx

```
src/pages/produtos/EditarProdutoPage.tsx
```

```tsx
export function EditarProdutoPage() {
  const { id } = useParams({ strict: false }) as { id: string };
  const navigate = useNavigate();
  const { data: produto, isLoading } = useProduto(id);
  const atualizar = useAtualizarProduto();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  // Preenche o formulário quando os dados chegarem da API
  useEffect(() => {
    if (produto) reset({ nome: produto.nome, descricao: produto.descricao,
                         preco: produto.preco, estoque: produto.estoque });
  }, [produto, reset]);

  const onSubmit = (data: FormData) =>
    atualizar.mutate({ id, ...data }, { onSuccess: () => navigate({ to: '/produtos' }) });

  if (isLoading) return <PageSpinner />;
  return <form onSubmit={handleSubmit(onSubmit)}>...</form>;
}
```

**Por que `useEffect` com `reset`?**
```
Componente monta → useForm inicializa com campos vazios
  → useProduto(id) → isLoading = true
  → API responde com { nome: "Camiseta", preco: 99.90, ... }
  → useEffect detecta mudança em `produto`
  → reset({ nome: "Camiseta", ... }) → formulário re-renderiza preenchido
```

### 10.4 Excluir Produto — Fluxo inline na Lista

Exclusão com confirmação em dois cliques na `ProdutosPage`:

```tsx
const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

// 1º clique — ativa confirmação
<Button onClick={() => setConfirmDelete(produto.id)}><Trash2 /></Button>

// 2º clique — efetivamente exclui
{confirmDelete === produto.id && (
  <Button variant="destructive" loading={excluir.isPending}
    onClick={() => excluir.mutate(produto.id, { onSuccess: () => setConfirmDelete(null) })}>
    Confirmar
  </Button>
)}
```

**Por que dois cliques?** Evita exclusões acidentais — o primeiro arma o estado, o
segundo confirma e dispara a mutação.

---

## 11. CRUD de Pedidos — Página a Página

### 11.1 Listar Pedidos — PedidosPage.tsx

```
src/pages/pedidos/PedidosPage.tsx
```

**O que faz:** Permite informar um `clienteId` (UUID), buscar os pedidos daquele cliente
e visualizá-los em cards com status, total e itens.

```tsx
export function PedidosPage() {
  const [clienteId, setClienteId] = useState('');
  const [busca, setBusca] = useState('');

  // Dois estados separados: clienteId = o que o usuário digita
  //                         busca     = valor confirmado ao clicar "Buscar"
  // Evita que a query seja re-executada a cada tecla

  const { data: pedidos, isLoading } = usePedidosPorCliente(busca);

  return (
    <div>
      <Card>
        <CardContent>
          <Input id="clienteId" label="ID do Cliente (UUID)"
                 value={clienteId} onChange={(e) => setClienteId(e.target.value)} />
          <Button variant="secondary" onClick={() => setBusca(clienteId.trim())}>
            Buscar
          </Button>
        </CardContent>
      </Card>

      {!busca && <EmptyState title="Informe um ID de cliente" />}
      {busca && isLoading && <PageSpinner />}
      {busca && !isLoading && pedidos?.length === 0 && <EmptyState title="Nenhum pedido encontrado" />}

      {pedidos?.map((pedido) => (
        <Card key={pedido.id}>
          <CardHeader>
            <Badge variant={statusVariant[pedido.status]}>{statusLabel[pedido.status]}</Badge>
            <span>{formatCurrency(pedido.total)}</span>
          </CardHeader>
          <CardContent>
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
  );
}
```

**Por que `pedido.itens ?? []`?**
A API de listagem por cliente pode retornar pedidos sem a lista de itens (otimização de
payload). O operador `??` garante que `.map()` receba um array vazio em vez de lançar erro.

### 11.2 Criar Pedido — NovoPedidoPage.tsx

```
src/pages/pedidos/NovoPedidoPage.tsx
```

**O que faz:** Formulário com `clienteId` + lista dinâmica de itens. Usa `useFieldArray`
para gerenciar o array. Exibe tela de sucesso inline com o `pedidoId` gerado.

```tsx
const schema = z.object({
  clienteId: z.string().uuid('ID do cliente deve ser um UUID válido'),
  itens: z.array(z.object({
    produtoId: z.string().min(1, 'Selecione um produto'),
    quantidade: z.number().int().min(1, 'Quantidade mínima é 1'),
  })).min(1, 'Adicione pelo menos um item'),
});

export function NovoPedidoPage() {
  const { data: produtos } = useProdutos();  // para popular o <select>
  const criar = useCriarPedido();
  const [pedidoId, setPedidoId] = useState<string | null>(null);

  const { register, handleSubmit, control, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { itens: [{ produtoId: '', quantidade: 1 }] },
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'itens' });
  const itensWatch = watch('itens');  // reativo — recalcula total a cada mudança

  const calcularTotal = () =>
    itensWatch.reduce((acc, item) => {
      const produto = produtos?.find((p) => p.id === item.produtoId);
      return acc + (produto?.preco ?? 0) * (item.quantidade || 0);
    }, 0);

  const onSubmit = (data: FormData) =>
    criar.mutate(data, { onSuccess: (res) => setPedidoId(res.pedidoId) });

  if (pedidoId) {
    return (
      <div>
        <p>Pedido criado com sucesso!</p>
        <code>{pedidoId}</code>
        <Button variant="outline" onClick={() => navigate({ to: '/pedidos' })}>Ver Pedidos</Button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input id="clienteId" label="ID do Cliente *" error={errors.clienteId?.message}
             {...register('clienteId')} />
      <Card>
        <CardHeader>
          <CardTitle>Itens do Pedido</CardTitle>
          <Button type="button" variant="outline" size="sm"
                  onClick={() => append({ produtoId: '', quantidade: 1 })}>
            Adicionar item
          </Button>
        </CardHeader>
        <CardContent>
          {fields.map((field, index) => (
            <div key={field.id}>
              <select {...register(`itens.${index}.produtoId`)}>
                {produtos?.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
              <Input type="number" min="1" {...register(`itens.${index}.quantidade`, { valueAsNumber: true })} />
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
      <Button type="submit" loading={criar.isPending}>Criar Pedido</Button>
    </form>
  );
}
```

**Por que `watch('itens')` e não `getValues('itens')`?**
`watch` é reativo — re-executa `calcularTotal()` a cada alteração de select ou quantidade.
`getValues` é uma leitura pontual e não causa re-renderização.

---

## 12. Consulta de CEP — ConsultaCepPage.tsx

```
src/pages/cep/ConsultaCepPage.tsx
```

**O que faz:** Permite ao usuário digitar um CEP, consultar via API (que por sua vez
chama o ViaCEP) e exibir os dados de endereço em cards organizados.

```tsx
function formatCep(value: string) {
  return value.replace(/\D/g, '').slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2');
  // "01310100" → "01310-100"
}

export function ConsultaCepPage() {
  const [input, setInput] = useState('');
  const [cepBusca, setCepBusca] = useState('');

  const { data, isLoading, isError, error, isFetching } = useCep(cepBusca);
  const apiError = isError ? extractApiError(error) : null;

  const handleBuscar = () => {
    const digits = input.replace(/\D/g, '');
    if (digits.length === 8) setCepBusca(digits);
  };

  // Exibe resultado em grid de campos
  const campos = data ? [
    { label: 'CEP',        value: data.cep },
    { label: 'Logradouro', value: data.logradouro || '—' },
    { label: 'Complemento',value: data.complemento || '—' },
    { label: 'Bairro',     value: data.bairro || '—' },
    { label: 'Cidade',     value: data.localidade },
    { label: 'UF',         value: data.uf },
    { label: 'DDD',        value: data.ddd },
    { label: 'IBGE',       value: data.ibge },
    { label: 'GIA',        value: data.gia || '—' },
    { label: 'SIAFI',      value: data.siafi },
  ] : [];

  return (
    <div className="space-y-8">
      {/* Cabeçalho com link para viacep.com.br */}
      <div className="flex items-center gap-3">
        <MapPin className="h-5 w-5 text-sky-600" />
        <div>
          <h1>Consulta de CEP</h1>
          <p>Consulta externa via <a href="https://viacep.com.br" target="_blank">ViaCEP</a></p>
        </div>
      </div>

      {/* Campo de busca com máscara */}
      <Card>
        <CardContent>
          <Input id="cep-input" placeholder="00000-000"
                 value={input}
                 onChange={(e) => setInput(formatCep(e.target.value))}
                 onKeyDown={(e) => e.key === 'Enter' && handleBuscar()} />
          <Button onClick={handleBuscar} disabled={input.replace(/\D/g, '').length !== 8}>
            <Search className="h-4 w-4" />
            Buscar
          </Button>
        </CardContent>
      </Card>

      {/* Estado de carregamento */}
      {isFetching && <Spinner />}

      {/* Erro — CEP não encontrado */}
      {isError && (
        <Card>
          <CardContent>
            <p className="text-red-600">{apiError?.title ?? 'CEP não encontrado.'}</p>
          </CardContent>
        </Card>
      )}

      {/* Resultado em grid */}
      {data && !isFetching && (
        <div className="grid gap-4 sm:grid-cols-3">
          {campos.map(({ label, value, icon: Icon }) => (
            <Card key={label}>
              <CardContent>
                <Icon className="h-4 w-4 text-sky-500" />
                <p className="text-xs text-gray-500">{label}</p>
                <p className="font-medium">{value}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Arquitetura da consulta de CEP — duas camadas de proxy:**

```
Browser (5173)
  ↓  GET /api/cep/01310100
  ↓
Vite Proxy
  ↓  GET http://localhost:5002/api/cep/01310100
  ↓
Pedidos API (.NET — porta 5002)
  ↓  CepEndpoints → ConsultarCepQuery → ConsultarCepQueryHandler
  ↓  ViaCepClient.ConsultarCepAsync("01310100")
  ↓  GET https://viacep.com.br/ws/01310100/json/
  ↓
ViaCEP (API pública)
  ↓  { "cep": "01310-100", "logradouro": "Av. Paulista", ... }
  ↓
Pedidos API mapeia para ViaCepDto → retorna 200
  ↓
Vite Proxy → Browser
  ↓
useCep → cache ['cep', '01310100'] → ConsultaCepPage re-renderiza
```

**Detalhes de UX implementados:**
- Máscara automática: `formatCep` aplica o padrão `XXXXX-XXX` enquanto o usuário digita
- Busca por `Enter`: `onKeyDown` intercepta a tecla Enter no campo
- Botão desabilitado até ter 8 dígitos: `disabled={digits.length !== 8}`
- `isFetching` vs `isLoading`: `isLoading` é `true` apenas na primeira busca; `isFetching` inclui re-buscas

---

## 13. Dashboard — DashboardPage.tsx

```
src/pages/dashboard/DashboardPage.tsx
```

**O que faz:** Reutiliza o cache de produtos para derivar métricas do catálogo e exibe
cartões de navegação rápida para todas as seções.

```tsx
export function DashboardPage() {
  const navigate = useNavigate();
  const { data: produtos, isLoading } = useProdutos();

  if (isLoading) return <PageSpinner />;

  // Métricas derivadas no cliente — sem endpoint específico de dashboard
  const totalProdutos  = produtos?.length ?? 0;
  const semEstoque     = produtos?.filter((p) => p.estoque === 0).length ?? 0;
  const estoqueTotal   = produtos?.reduce((acc, p) => acc + p.estoque, 0) ?? 0;
  const valorCatalogo  = produtos?.reduce((acc, p) => acc + p.preco * p.estoque, 0) ?? 0;

  return (
    <div>
      {/* Métricas */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card><CardContent><p>Total de Produtos</p><p>{totalProdutos}</p></CardContent></Card>
        <Card><CardContent><p>Sem Estoque</p><p>{semEstoque}</p></CardContent></Card>
        <Card><CardContent><p>Unidades em Estoque</p><p>{estoqueTotal}</p></CardContent></Card>
        <Card><CardContent><p>Valor do Catálogo</p><p>{formatCurrency(valorCatalogo)}</p></CardContent></Card>
      </div>

      {/* Navegação rápida */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card><CardContent>
          <p>Produtos</p>
          <Button onClick={() => navigate({ to: '/produtos' })}>Acessar Produtos</Button>
        </CardContent></Card>
        <Card><CardContent>
          <p>Pedidos</p>
          <Button onClick={() => navigate({ to: '/pedidos' })}>Acessar Pedidos</Button>
        </CardContent></Card>
        <Card><CardContent>
          <p>Consulta de CEP</p>
          <Button onClick={() => navigate({ to: '/cep' })}>Consultar CEP</Button>
        </CardContent></Card>
      </div>
    </div>
  );
}
```

**Compartilhamento de cache:**
```
Usuário acessa /produtos → useProdutos() → GET /api/produtos/ → cache ['produtos'] preenchido
Usuário navega para /    → useProdutos() → cache ainda válido (staleTime 30s) → ZERO requisição
Usuário cria produto     → invalidateQueries(['produtos']) → Dashboard re-busca automaticamente
```

As métricas são **derivadas no cliente** — não existe endpoint `/api/dashboard`. O
frontend transforma os dados já disponíveis com `.filter()`, `.reduce()` e `formatCurrency()`.

---

## 14. Fluxo Completo de Dados — Do Clique à API

### 14.1 Criar Produto

```
Usuário clica "Criar Produto"
  ↓
handleSubmit() → zodResolver.parse(values)
  ├── FALHA → errors exibidos → formulário não submete
  └── SUCESSO → onSubmit(data)
        ↓
      criar.mutate(data)  [useCriarProduto]
        → isPending = true → Button mostra spinner
        ↓
      produtosService.criar(data)
        ↓
      produtosApi.post('/', data)  [Axios]
        → Content-Type: application/json
        ↓
      Vite Proxy → POST http://localhost:5001/api/produtos/
        ↓
      API .NET Produtos
        → ProdutosEndpoints → CriarProdutoCommandHandler
        → FluentValidation → Produto.Criar() → SaveChanges()
        → 201 Created + { id, nome, ... }
        ↓
      onSuccess()
        → invalidateQueries(['produtos'])
        → navigate({ to: '/produtos' })
        ↓
      ProdutosPage → useProdutos() re-busca → lista atualizada
```

### 14.2 Criar Pedido

```
Usuário clica "Criar Pedido"
  ↓
handleSubmit() → zodResolver (clienteId UUID + itens.min(1))
  └── SUCESSO → onSubmit(data)
        ↓
      criar.mutate({ clienteId, itens })  [useCriarPedido]
        ↓
      pedidosApi.post('/', data)  [Axios]
        ↓
      Vite Proxy → POST http://localhost:5002/api/pedidos/
        ↓
      API .NET Pedidos
        → CriarPedidoCommandHandler
        → Chama Produtos API (HTTP síncrono) → valida estoque
        → Pedido.Criar() → SaveChanges()
        → Publica PedidoConfirmadoEvent → RabbitMQ → Produtos debita estoque
        → 201 Created + { pedidoId: "guid" }
        ↓
      onSuccess(res)
        → setPedidoId(res.pedidoId) → tela de sucesso exibida inline
        → invalidateQueries(['pedidos', 'cliente', clienteId])
```

### 14.3 Consultar CEP

```
Usuário digita "01310-100" → formatCep aplica máscara
Usuário clica "Buscar" (ou pressiona Enter)
  ↓
setCepBusca("01310100")
  ↓
useCep("01310100")
  → enabled: digits.length === 8 → true
  → queryKey: ['cep', '01310100']
  → cache hit? → SIM → exibe imediatamente (staleTime 1h)
               → NÃO → isFetching = true → Spinner exibido
        ↓
cepService.consultar("01310100")
  ↓
cepApi.get('/01310100')  [Axios]
  ↓
Vite Proxy → GET http://localhost:5002/api/cep/01310100
  ↓
API .NET Pedidos
  → CepEndpoints → ConsultarCepQuery → ConsultarCepQueryHandler
  → ViaCepClient → GET https://viacep.com.br/ws/01310100/json/
  → mapeia ViaCepResponse para ViaCepDto
     ├── { "erro": true } → 404 + Error("Cep.NaoEncontrado", ...)
     └── OK → 200 + ViaCepDto
  ↓
useCep → isError ou data preenchido
  ├── isError → extractApiError → mensagem exibida no card de erro
  └── data    → grid de campos renderizado
```

---

## 15. Utilitários — lib/utils.ts

```
src/lib/utils.ts
```

```typescript
// Combina classes Tailwind com condicionais e resolução de conflitos
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Formata número para moeda brasileira
export function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}
// formatCurrency(99.9)   → "R$ 99,90"
// formatCurrency(1234.5) → "R$ 1.234,50"

// Formata string ISO 8601 para data/hora legível em pt-BR
export function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
    .format(new Date(dateString));
}
// formatDate("2026-05-16T04:16:16.661") → "16/05/2026, 01:16"
```

`Intl.NumberFormat` e `Intl.DateTimeFormat` são APIs nativas do browser — sem biblioteca
externa — e respeitam as convenções de localidade (vírgula decimal, ponto de milhar).

| Função | Usado em |
|--------|----------|
| `cn()` | Todos os componentes UI (Button, Input, Card, Badge, Navbar, ...) |
| `formatCurrency()` | ProdutosPage, PedidosPage, NovoPedidoPage, DashboardPage |
| `formatDate()` | ProdutosPage (`criadoEm`), PedidosPage (`criadoEm` do pedido) |

---

## 16. Diagrama de Dependências entre Camadas

```
main.tsx
  ├── router.ts
  │     └── RootLayout → Navbar (4 itens: Dashboard, Produtos, Pedidos, CEP) + Outlet
  │
  └── QueryClient (TanStack Query — staleTime 30s, retry 1x)
        │
        └── PÁGINAS
              │
              ├── DashboardPage.tsx
              │     └── useProdutos()              → produtosService.listar()
              │
              ├── ProdutosPage.tsx
              │     ├── useProdutos()              → produtosService.listar()
              │     └── useExcluirProduto()        → produtosService.excluir(id)
              │
              ├── NovoProdutoPage.tsx
              │     └── useCriarProduto()          → produtosService.criar(payload)
              │
              ├── EditarProdutoPage.tsx
              │     ├── useProduto(id)             → produtosService.obterPorId(id)
              │     └── useAtualizarProduto()      → produtosService.atualizar(id, payload)
              │
              ├── PedidosPage.tsx
              │     └── usePedidosPorCliente(id)   → pedidosService.listarPorCliente(id)
              │
              ├── NovoPedidoPage.tsx
              │     ├── useProdutos()              → produtosService.listar() (para o <select>)
              │     └── useCriarPedido()           → pedidosService.criar(payload)
              │
              └── ConsultaCepPage.tsx
                    └── useCep(cep)                → cepService.consultar(cep)

HOOKS
  ├── useProdutos.ts  → produtosService  (src/services/produtosService.ts)
  ├── usePedidos.ts   → pedidosService   (src/services/pedidosService.ts)
  └── useCep.ts       → cepService       (src/services/cepService.ts)
                                           staleTime: 1h | retry: false

SERVIÇOS
  ├── produtosService.ts → produtosApi  (baseURL: '/api/produtos')
  ├── pedidosService.ts  → pedidosApi   (baseURL: '/api/pedidos')
  └── cepService.ts      → cepApi       (baseURL: '/api/cep')

HTTP CLIENT (src/lib/http.ts — 3 instâncias Axios)
  ├── produtosApi → axios.create({ baseURL: '/api/produtos' })
  ├── pedidosApi  → axios.create({ baseURL: '/api/pedidos' })
  └── cepApi      → axios.create({ baseURL: '/api/cep' })

PROXY VITE (vite.config.ts)
  ├── /api/produtos → http://localhost:5001  (Produtos API)
  ├── /api/pedidos  → http://localhost:5002  (Pedidos API)
  └── /api/cep      → http://localhost:5002  (Pedidos API — que integra com viacep.com.br)

TIPOS
  ├── src/types/produto.ts → Produto, CriarProdutoRequest, AtualizarProdutoRequest
  ├── src/types/pedido.ts  → Pedido, CriarPedidoRequest, PedidoCriado,
  │                          ItemPedido, ItemPedidoDetalhe, StatusPedidoNumerico
  └── src/types/cep.ts     → ViaCep

COMPONENTES UI (src/components/ui/)
  ├── Button       → todas as ações (navegação, submit, exclusão, busca de CEP)
  ├── Input        → todos os formulários + busca por clienteId e CEP
  ├── Card / CardHeader / CardTitle / CardContent
  │                → Dashboard, Produtos, Pedidos, ConsultaCepPage
  ├── Badge        → estoque em Produtos, status em Pedidos
  ├── EmptyState   → estados vazios em Produtos e Pedidos
  └── Spinner / PageSpinner
                   → carregamento em todas as páginas + isFetching em ConsultaCepPage

UTILITÁRIOS (src/lib/utils.ts)
  ├── cn()             → todos os componentes UI
  ├── formatCurrency() → Produtos, Pedidos, Dashboard
  └── formatDate()     → Produtos, Pedidos
```

---

*Documentação gerada para o projeto `Arquitetura.Frontend`. Para a documentação completa
do backend e da infraestrutura, consulte `Arquitetura.Server/BACKEND.md` e o
`README.md` na raiz da solução.*
