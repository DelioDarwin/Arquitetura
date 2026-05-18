import { useNavigate } from '@tanstack/react-router';
import { Package, ShoppingCart, TrendingUp, AlertTriangle, MapPin } from 'lucide-react';
import { useProdutos } from '@/hooks/useProdutos';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { PageSpinner } from '@/components/ui/Spinner';
import { formatCurrency } from '@/lib/utils';

export function DashboardPage() {
  const navigate = useNavigate();
  const { data: produtos, isLoading } = useProdutos();

  if (isLoading) return <PageSpinner />;

  const totalProdutos = produtos?.length ?? 0;
  const semEstoque = produtos?.filter((p) => p.estoque === 0).length ?? 0;
  const estoqueTotal = produtos?.reduce((a, p) => a + p.estoque, 0) ?? 0;
  const valorCatalogo = produtos?.reduce((a, p) => a + p.preco * p.estoque, 0) ?? 0;

  const stats = [
    {
      label: 'Produtos Cadastrados',
      value: totalProdutos,
      icon: Package,
      color: 'text-indigo-600',
      bg: 'bg-indigo-50',
    },
    {
      label: 'Unidades em Estoque',
      value: estoqueTotal,
      icon: TrendingUp,
      color: 'text-emerald-600',
      bg: 'bg-emerald-50',
    },
    {
      label: 'Sem Estoque',
      value: semEstoque,
      icon: AlertTriangle,
      color: 'text-amber-600',
      bg: 'bg-amber-50',
    },
    {
      label: 'Valor do Catlogo',
      value: formatCurrency(valorCatalogo),
      icon: ShoppingCart,
      color: 'text-purple-600',
      bg: 'bg-purple-50',
    },
  ];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500">Visão geral da plataforma</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map(({ label, value, icon: Icon, color, bg }) => (
          <Card key={label}>
            <CardContent className="flex items-center gap-4 py-5">
              <div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ${bg}`}>
                <Icon className={`h-6 w-6 ${color}`} />
              </div>
              <div>
                <p className="text-sm text-gray-500">{label}</p>
                <p className="text-2xl font-bold text-gray-900">{value}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <Card>
          <CardContent className="flex flex-col items-start gap-3 py-6">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-50">
              <Package className="h-5 w-5 text-indigo-600" />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">Gerenciar Produtos</h3>
              <p className="text-sm text-gray-500">
                Crie, edite e remova produtos do catlogo.
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={() => navigate({ to: '/produtos' })}>
              Ir para Produtos
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex flex-col items-start gap-3 py-6">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-50">
              <ShoppingCart className="h-5 w-5 text-emerald-600" />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">Gerenciar Pedidos</h3>
              <p className="text-sm text-gray-500">
                Crie novos pedidos e consulte o histrico por cliente.
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={() => navigate({ to: '/pedidos' })}>
              Ir para Pedidos
            </Button>
          </CardContent>
        </Card>
      </div>

      {semEstoque > 0 && (
        <div className="flex items-center gap-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3">
          <AlertTriangle className="h-5 w-5 shrink-0 text-amber-600" />
          <p className="text-sm text-amber-800">
            <strong>{semEstoque}</strong> produto(s) sem estoque. Atualize o inventrio.
          </p>
          <Button
            variant="outline"
            size="sm"
            className="ml-auto"
            onClick={() => navigate({ to: '/produtos' })}
          >
            Ver produtos
          </Button>
        </div>
      )}

      {/* ── Serviços Externos ── */}
      <div>
        <h2 className="mb-1 text-lg font-semibold text-gray-900">Serviços Externos</h2>
        <p className="mb-4 text-sm text-gray-500">Integrações com APIs de terceiros disponíveis na plataforma.</p>
        <div className="flex justify-center">
          <button
            type="button"
            className="w-full max-w-sm rounded-xl text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            onClick={() => navigate({ to: '/cep' })}
          >
            <Card className="cursor-pointer transition-shadow hover:shadow-md">
              <CardContent className="flex flex-col items-center gap-4 py-8 text-center">
                <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-sky-100">
                  <MapPin className="h-7 w-7 text-sky-600" />
                </div>
                <div>
                  <h3 className="text-base font-semibold text-gray-900">Consulta de CEP</h3>
                  <p className="mt-1 text-sm text-gray-500">
                    Busque endereços completos pelo CEP via ViaCEP.
                  </p>
                </div>
                <span className="inline-flex items-center rounded-full bg-sky-50 px-3 py-1 text-xs font-medium text-sky-700 ring-1 ring-inset ring-sky-200">
                  ViaCEP · API Gratuita
                </span>
              </CardContent>
            </Card>
          </button>
        </div>
      </div>
    </div>
  );
}
