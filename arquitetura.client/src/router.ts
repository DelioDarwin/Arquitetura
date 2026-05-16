import { createRouter, createRoute, createRootRoute } from '@tanstack/react-router';
import { RootLayout } from '@/components/layout/RootLayout';
import { DashboardPage } from '@/pages/dashboard/DashboardPage';
import { ProdutosPage } from '@/pages/produtos/ProdutosPage';
import { NovoProdutoPage } from '@/pages/produtos/NovoProdutoPage';
import { EditarProdutoPage } from '@/pages/produtos/EditarProdutoPage';
import { PedidosPage } from '@/pages/pedidos/PedidosPage';
import { NovoPedidoPage } from '@/pages/pedidos/NovoPedidoPage';

const rootRoute = createRootRoute({ component: RootLayout });

const indexRoute = createRoute({ getParentRoute: () => rootRoute, path: '/', component: DashboardPage });
const produtosRoute = createRoute({ getParentRoute: () => rootRoute, path: '/produtos', component: ProdutosPage });
const novoProdutoRoute = createRoute({ getParentRoute: () => rootRoute, path: '/produtos/novo', component: NovoProdutoPage });
const editarProdutoRoute = createRoute({ getParentRoute: () => rootRoute, path: '/produtos/$id/editar', component: EditarProdutoPage });
const pedidosRoute = createRoute({ getParentRoute: () => rootRoute, path: '/pedidos', component: PedidosPage });
const novoPedidoRoute = createRoute({ getParentRoute: () => rootRoute, path: '/pedidos/novo', component: NovoPedidoPage });

const routeTree = rootRoute.addChildren([
  indexRoute,
  produtosRoute,
  novoProdutoRoute,
  editarProdutoRoute,
  pedidosRoute,
  novoPedidoRoute,
]);

export const router = createRouter({ routeTree });

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}

