import { useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import { Plus, Pencil, Trash2, UserRound } from 'lucide-react';
import { useClientes, useExcluirCliente } from '@/hooks/userClientes';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { EmptyState } from '@/components/ui/EmptyState';
import { PageSpinner } from '@/components/ui/Spinner';
import { formatDate } from '@/lib/utils';

export function ClientesPage() {
  const navigate = useNavigate();
  const { data: clientes, isLoading, isError } = useClientes();
  const excluir = useExcluirCliente();
  const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

  if (isLoading) return <PageSpinner />;

  if (isError)
    return (
      <p className="text-center text-sm text-red-600">
        Erro ao carregar clientes. Verifique se a API está disponível.
      </p>
    );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Clientes</h1>
          <p className="text-sm text-gray-500">{clientes?.length ?? 0} cliente(s) cadastrado(s)</p>
        </div>
        <Button onClick={() => navigate({ to: '/clientes/novo' })}>
          <Plus className="h-4 w-4" />
          Novo Cliente
        </Button>
      </div>

      {!clientes || clientes.length === 0 ? (
        <EmptyState
          title="Nenhum cliente cadastrado"
          description="Comece cadastrando o primeiro cliente."
          action={
            <Button onClick={() => navigate({ to: '/clientes/novo' })}>
              <Plus className="h-4 w-4" />
              Criar Cliente
            </Button>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {clientes.map((cliente) => (
            <Card key={cliente.id}>
              <CardHeader>
                <div className="flex items-center gap-2 min-w-0">
                  <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-indigo-50">
                    <UserRound className="h-5 w-5 text-indigo-600" />
                  </div>
                  <CardTitle className="truncate">{cliente.nome}</CardTitle>
                </div>
              </CardHeader>
              <CardContent>
                <p className="mb-1 text-sm text-gray-600 truncate">{cliente.email}</p>
                <p className="mb-1 text-sm text-gray-500">CPF: {cliente.cpf}</p>
                <p className="mb-4 text-xs text-gray-400">
                  Cadastrado em {formatDate(cliente.criadoEm)}
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1"
                    onClick={() => navigate({ to: '/clientes/$id/editar', params: { id: cliente.id } })}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                    Editar
                  </Button>
                  {confirmDelete === cliente.id ? (
                    <Button
                      variant="destructive"
                      size="sm"
                      loading={excluir.isPending}
                      onClick={() =>
                        excluir.mutate(cliente.id, { onSuccess: () => setConfirmDelete(null) })
                      }
                    >
                      Confirmar
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setConfirmDelete(cliente.id)}
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
