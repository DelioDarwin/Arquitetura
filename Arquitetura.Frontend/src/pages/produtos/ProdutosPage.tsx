import { useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import { Plus, Pencil, Trash2, Package } from 'lucide-react';
import { useProdutos, useExcluirProduto } from '@/hooks/useProdutos';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { PageSpinner } from '@/components/ui/Spinner';
import { formatCurrency } from '@/lib/utils';

export function ProdutosPage() {
  const navigate = useNavigate();
  const { data: produtos, isLoading, isError } = useProdutos();
  const excluir = useExcluirProduto();
  const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

  if (isLoading) return <PageSpinner />;

  if (isError)
    return (
      <p className="text-center text-sm text-red-600">
        Erro ao carregar produtos. Verifique se a API est disponvel.
      </p>
    );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Produtos</h1>
          <p className="text-sm text-gray-500">{produtos?.length ?? 0} produto(s) cadastrado(s)</p>
        </div>
        <Button onClick={() => navigate({ to: '/produtos/novo' })}>
          <Plus className="h-4 w-4" />
          Novo Produto
        </Button>
      </div>

      {!produtos || produtos.length === 0 ? (
        <EmptyState
          title="Nenhum produto cadastrado"
          description="Comece criando o primeiro produto do catlogo."
          action={
            <Button onClick={() => navigate({ to: '/produtos/novo' })}>
              <Plus className="h-4 w-4" />
              Criar Produto
            </Button>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {produtos.map((produto) => (
            <Card key={produto.id}>
              <CardHeader>
                <div className="flex items-center gap-2 min-w-0">
                  <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-indigo-50">
                    <Package className="h-5 w-5 text-indigo-600" />
                  </div>
                  <CardTitle className="truncate">{produto.nome}</CardTitle>
                </div>
                <Badge variant={produto.estoque > 0 ? 'success' : 'destructive'}>
                  {produto.estoque > 0 ? `${produto.estoque} un.` : 'Sem estoque'}
                </Badge>
              </CardHeader>
              <CardContent>
                <p className="mb-3 line-clamp-2 text-sm text-gray-500">{produto.descricao}</p>
                <p className="mb-4 text-lg font-bold text-indigo-600">{formatCurrency(produto.preco)}</p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1"
                    onClick={() => navigate({ to: '/produtos/$id/editar', params: { id: produto.id } })}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                    Editar
                  </Button>
                  {confirmDelete === produto.id ? (
                    <Button
                      variant="destructive"
                      size="sm"
                      loading={excluir.isPending}
                      onClick={() => excluir.mutate(produto.id, { onSuccess: () => setConfirmDelete(null) })}
                    >
                      Confirmar
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setConfirmDelete(produto.id)}
                    >
                      <Trash2 className="h-3.5 w-3.5 text-red-500" />
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
