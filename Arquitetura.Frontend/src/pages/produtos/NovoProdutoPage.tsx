import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from '@tanstack/react-router';
import { ArrowLeft } from 'lucide-react';
import { useCriarProduto } from '@/hooks/useProdutos';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';

const schema = z.object({
  nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  descricao: z.string().min(5, 'Descricao deve ter pelo menos 5 caracteres'),
  preco: z.number().positive('Preco deve ser positivo'),
  estoque: z.number().int().min(0, 'Estoque nao pode ser negativo'),
});

type FormData = z.infer<typeof schema>;

export function NovoProdutoPage() {
  const navigate = useNavigate();
  const criar = useCriarProduto();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = (data: FormData) => {
    criar.mutate(data, {
      onSuccess: () => navigate({ to: '/produtos' }),
    });
  };

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate({ to: '/produtos' })}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Novo Produto</h1>
          <p className="text-sm text-gray-500">Preencha as informacoes do produto</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do produto</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              id="nome"
              label="Nome *"
              placeholder="Ex: Camiseta Polo"
              error={errors.nome?.message}
              {...register('nome')}
            />
            <div className="flex flex-col gap-1">
              <label htmlFor="descricao" className="text-sm font-medium text-gray-700">
                Descricao *
              </label>
              <textarea
                id="descricao"
                rows={3}
                placeholder="Descreva o produto..."
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
                placeholder="0,00"
                error={errors.preco?.message}
                {...register('preco', { valueAsNumber: true })}
              />
              <Input
                id="estoque"
                label="Estoque *"
                type="number"
                placeholder="0"
                error={errors.estoque?.message}
                {...register('estoque', { valueAsNumber: true })}
              />
            </div>

            {criar.isError && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
                Erro ao criar produto. Verifique os dados e tente novamente.
              </p>
            )}

            <div className="flex justify-end gap-3 pt-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate({ to: '/produtos' })}
              >
                Cancelar
              </Button>
              <Button type="submit" loading={criar.isPending}>
                Criar Produto
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
