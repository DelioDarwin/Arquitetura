import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, useParams } from '@tanstack/react-router';
import { ArrowLeft } from 'lucide-react';
import { useCliente, useAtualizarCliente } from '@/hooks/userClientes';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { PageSpinner } from '@/components/ui/Spinner';

const schema = z.object({
  nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  email: z.string().email('E-mail inválido'),
  cpf: z.string().min(11, 'CPF deve ter 11 dígitos').max(14, 'CPF inválido'),
});

type FormData = z.infer<typeof schema>;

export function EditarClientePage() {
  const { id } = useParams({ strict: false }) as { id: string };
  const navigate = useNavigate();
  const { data: cliente, isLoading } = useCliente(id);
  const atualizar = useAtualizarCliente();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  useEffect(() => {
    if (cliente) {
      reset({
        nome: cliente.nome,
        email: cliente.email,
        cpf: cliente.cpf,
      });
    }
  }, [cliente, reset]);

  const onSubmit = (data: FormData) => {
    atualizar.mutate(
      { id, ...data },
      { onSuccess: () => navigate({ to: '/clientes' }) }
    );
  };

  if (isLoading) return <PageSpinner />;

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate({ to: '/clientes' })}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Editar Cliente</h1>
          <p className="text-sm text-gray-500">Atualize os Dados do cliente</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do cliente</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              id="nome"  
              label="Nome *"
              placeholder="Ex: João da Silva"
              error={errors.nome?.message}
              {...register('nome')}
            />
            <Input
              id="email"
              label="E-mail *"
              type="email"
              placeholder="joao@email.com"
              error={errors.email?.message}
              {...register('email')}
            />
            <Input
              id="cpf"
              label="CPF *"
              placeholder="000.000.000-00"
              error={errors.cpf?.message}
              {...register('cpf')}
            />

            {atualizar.isError && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
                Erro ao atualizar cliente. Verifique os dados e tente novamente.
              </p>
            )}

            <div className="flex gap-3 pt-2">
              <Button
                type="button"
                variant="outline"
                className="flex-1"
                onClick={() => navigate({ to: '/clientes' })}
              >
                Cancelar
              </Button>
              <Button type="submit" className="flex-1" loading={atualizar.isPending}>
                Salvar 
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
