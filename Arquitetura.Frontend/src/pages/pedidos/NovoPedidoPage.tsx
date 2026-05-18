import { useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from '@tanstack/react-router';
import { ArrowLeft, Plus, Trash2, ShoppingCart } from 'lucide-react';
import { useCriarPedido } from '@/hooks/usePedidos';
import { useProdutos } from '@/hooks/useProdutos';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { formatCurrency } from '@/lib/utils';

const schema = z.object({
  clienteId: z.string().uuid('Informe um UUID vlido para o cliente'),
  itens: z
    .array(
      z.object({
        produtoId: z.string().uuid('Selecione um produto'),
        quantidade: z.number().int().positive('Quantidade deve ser positiva'),
      })
    )
    .min(1, 'Adicione pelo menos um item'),
});

type FormData = z.infer<typeof schema>;

export function NovoPedidoPage() {
  const navigate = useNavigate();
  const criar = useCriarPedido();
  const { data: produtos } = useProdutos();
  const [sucesso, setSucesso] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    control,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { itens: [{ produtoId: '', quantidade: 1 }] },
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'itens' });
  const itensWatch = watch('itens');

  const calcularTotal = () => {
    if (!produtos || !itensWatch) return 0;
    return itensWatch.reduce((acc, item) => {
      const produto = produtos.find((p) => p.id === item.produtoId);
      return acc + (produto?.preco ?? 0) * (item.quantidade || 0);
    }, 0);
  };

  const onSubmit = (data: FormData) => {
    criar.mutate(data, {
      onSuccess: (res) => setSucesso(res.pedidoId),
    });
  };

  if (sucesso) {
    return (
      <div className="mx-auto max-w-lg">
        <Card>
          <CardContent className="flex flex-col items-center py-12 text-center">
            <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-100">
              <ShoppingCart className="h-8 w-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-gray-900">Pedido criado!</h2>
            <p className="mt-2 text-sm text-gray-500">
              ID do pedido: <span className="font-mono text-gray-800">{sucesso}</span>
            </p>
            <div className="mt-6 flex gap-3">
              <Button variant="outline" onClick={() => navigate({ to: '/pedidos' })}>
                Ver Pedidos
              </Button>
              <Button onClick={() => { setSucesso(null); }}>
                Novo Pedido
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate({ to: '/pedidos' })}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Novo Pedido</h1>
          <p className="text-sm text-gray-500">Selecione os produtos e quantidades</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados do cliente</CardTitle>
          </CardHeader>
          <CardContent>
            <Input
              id="clienteId"
              label="ID do Cliente (UUID) *"
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
              error={errors.clienteId?.message}
              {...register('clienteId')}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Itens do Pedido</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => append({ produtoId: '', quantidade: 1 })}
            >
              <Plus className="h-3.5 w-3.5" />
              Adicionar item
            </Button>
          </CardHeader>
          <CardContent>
            {errors.itens?.root && (
              <p className="mb-3 text-sm text-red-600">{errors.itens.root.message}</p>
            )}
            <div className="space-y-3">
              {fields.map((field, index) => {
                const produtoSelecionado = produtos?.find(
                  (p) => p.id === itensWatch?.[index]?.produtoId
                );
                return (
                  <div key={field.id} className="flex items-start gap-3">
                    <div className="flex-1">
                      <label className="mb-1 block text-sm font-medium text-gray-700">
                        Produto *
                      </label>
                      <select
                        className="h-9 w-full rounded-md border border-gray-300 bg-white px-3 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        {...register(`itens.${index}.produtoId`)}
                      >
                        <option value="">Selecione um produto...</option>
                        {produtos?.map((p) => (
                          <option key={p.id} value={p.id} disabled={p.estoque === 0}>
                            {p.nome}  {formatCurrency(p.preco)}
                            {p.estoque === 0 ? ' (sem estoque)' : ''}
                          </option>
                        ))}
                      </select>
                      {errors.itens?.[index]?.produtoId && (
                        <p className="mt-1 text-xs text-red-600">
                          {errors.itens[index].produtoId?.message}
                        </p>
                      )}
                    </div>
                    <div className="w-24">
                      <Input
                        id={`itens.${index}.quantidade`}
                        label="Qtd *"
                        type="number"
                        min={1}
                        max={produtoSelecionado?.estoque}
                        error={errors.itens?.[index]?.quantidade?.message}
                        {...register(`itens.${index}.quantidade`, { valueAsNumber: true })}
                      />
                    </div>
                    {produtoSelecionado && (
                      <div className="mt-6 text-right text-sm">
                        <p className="font-medium text-indigo-600">
                          {formatCurrency(
                            produtoSelecionado.preco * (itensWatch?.[index]?.quantidade || 0)
                          )}
                        </p>
                      </div>
                    )}
                    <button
                      type="button"
                      onClick={() => remove(index)}
                      disabled={fields.length === 1}
                      className="mt-6 text-gray-400 hover:text-red-500 disabled:opacity-30"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                );
              })}
            </div>

            <div className="mt-4 flex justify-end border-t border-gray-100 pt-4">
              <div className="text-right">
                <p className="text-sm text-gray-500">Total estimado</p>
                <p className="text-xl font-bold text-indigo-600">{formatCurrency(calcularTotal())}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        {criar.isError && (
          <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
            Erro ao criar pedido. Verifique se os produtos tm estoque suficiente.
          </p>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate({ to: '/pedidos' })}>
            Cancelar
          </Button>
          <Button type="submit" loading={criar.isPending}>
            <ShoppingCart className="h-4 w-4" />
            Confirmar Pedido
          </Button>
        </div>
      </form>
    </div>
  );
}
