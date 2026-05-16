import { useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import { Search, Plus, ShoppingCart } from 'lucide-react';
import { usePedidosPorCliente } from '@/hooks/usePedidos';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { PageSpinner } from '@/components/ui/Spinner';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { StatusPedidoNumerico } from '@/types/pedido';

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

export function PedidosPage() {
  const navigate = useNavigate();
  const [clienteId, setClienteId] = useState('');
  const [busca, setBusca] = useState('');

  const { data: pedidos, isLoading, isFetching } = usePedidosPorCliente(busca);

  const handleBuscar = (e: React.FormEvent) => {
    e.preventDefault();
    setBusca(clienteId.trim());
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Pedidos</h1>
          <p className="text-sm text-gray-500">Consulte pedidos por cliente</p>
        </div>
        <Button onClick={() => navigate({ to: '/pedidos/novo' })}>
          <Plus className="h-4 w-4" />
          Novo Pedido
        </Button>
      </div>

      <Card>
        <CardContent className="py-4">
          <form onSubmit={handleBuscar} className="flex gap-3">
            <Input
              id="clienteId"
              placeholder="Informe o ID do cliente (GUID)..."
              value={clienteId}
              onChange={(e) => setClienteId(e.target.value)}
              className="flex-1"
            />
            <Button type="submit" variant="secondary" loading={isFetching && !!busca}>
              <Search className="h-4 w-4" />
              Buscar
            </Button>
          </form>
        </CardContent>
      </Card>

      {!busca ? (
        <EmptyState
          title="Informe um ID de cliente"
          description="Digite o ID do cliente para listar seus pedidos."
        />
      ) : isLoading ? (
        <PageSpinner />
      ) : !pedidos || pedidos.length === 0 ? (
        <EmptyState
          title="Nenhum pedido encontrado"
          description={`Nenhum pedido para o cliente ${busca}.`}
          action={
            <Button onClick={() => navigate({ to: '/pedidos/novo' })}>
              <Plus className="h-4 w-4" />
              Criar Pedido
            </Button>
          }
        />
      ) : (
        <div className="space-y-4">
          <p className="text-sm text-gray-500">{pedidos.length} pedido(s) encontrado(s)</p>
          {pedidos.map((pedido) => (
            <Card key={pedido.id}>
              <CardHeader>
                <div className="flex items-center gap-2">
                  <ShoppingCart className="h-4 w-4 text-gray-400" />
                  <CardTitle className="font-mono text-sm">{pedido.id}</CardTitle>
                </div>
                <Badge variant={statusVariant[pedido.status]}>{statusLabel[pedido.status]}</Badge>
              </CardHeader>
              <CardContent>
                <div className="mb-3 grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-gray-500">Total</p>
                    <p className="font-semibold text-indigo-600">{formatCurrency(pedido.total)}</p>
                  </div>
                  <div>
                    <p className="text-gray-500">Criado em</p>
                    <p className="font-medium">{formatDate(pedido.criadoEm)}</p>
                  </div>
                </div>
                <div className="rounded-md bg-gray-50 p-3">
                  <p className="mb-2 text-xs font-medium text-gray-600 uppercase tracking-wide">Itens</p>
                  <div className="space-y-1">
                    {(pedido.itens ?? []).map((item) => (
                      <div key={item.produtoId} className="flex justify-between text-sm">
                        <span className="text-gray-700">
                          {item.nomeProduto}  {item.quantidade}
                        </span>
                        <span className="font-medium">{formatCurrency(item.subtotal)}</span>
                      </div>
                    ))}
                    {!pedido.itens && (
                      <p className="text-xs text-gray-400">Detalhes não disponíveis na listagem</p>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
