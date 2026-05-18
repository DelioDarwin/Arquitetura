import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, useParams } from '@tanstack/react-router';
import { ArrowLeft } from 'lucide-react';
import { useProduto, useAtualizarProduto } from '@/hooks/useProdutos';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { PageSpinner } from '@/components/ui/Spinner';

const schema = z.object({
  nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  descricao: z.string().min(5, 'Descricao deve ter pelo menos 5 caracteres'),
  preco: z.number().positive('Preco deve ser positivo'),
  estoque: z.number().int().min(0, 'Estoque nao pode ser negativo'),
});

type FormData = z.infer<typeof schema>;

export function EditarProdutoPage() {
  const { id } = useParams({ strict: false }) as { id: string };
  const navigate = useNavigate();
  const { data: produto, isLoading } = useProduto(id);
  const atualizar = useAtualizarProduto();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

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

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate({ to: '/produtos' })}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Editar Produto</h1>
          <p className="text-sm text-gray-500">{produto?.nome}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do produto</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input id="nome" label="Nome *" error={errors.nome?.message} {...register('nome')} />
            <div className="flex flex-col gap-1">
              <label htmlFor="descricao" className="text-sm font-medium text-gray-700">
                Descricao *
              </label>
              <textarea
                id="descricao"
                rows={3}
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                {...register('descricao')}
              />
              {errors.descricao && (
                <p className="text-xs text-red-600">{errors.descricao.message}</p>
              )}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input
                id="preco"
                label="Preco (R$) *"
                type="number"
                step="0.01"
                error={errors.preco?.message}
                {...register('preco', { valueAsNumber: true })}
              />
              <Input
                id="estoque"
                label="Estoque *"
                type="number"
                error={errors.estoque?.message}
                {...register('estoque', { valueAsNumber: true })}
              />
            </div>
            {atualizar.isError && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
                Erro ao atualizar produto.
              </p>
            )}
            <div className="flex justify-end gap-3 pt-2">
              <Button type="button" variant="outline" onClick={() => navigate({ to: '/produtos' })}>
                Cancelar
              </Button>
              <Button type="submit" loading={atualizar.isPending}>
                Salvar Alteraes
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
