import { useState } from 'react';
import { MapPin, Search, Building2, Map, Hash, Phone } from 'lucide-react';
import { useCep } from '@/hooks/useCep';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent } from '@/components/ui/Card';
import { Spinner } from '@/components/ui/Spinner';
import { extractApiError } from '@/lib/http';

function formatCep(value: string) {
  return value.replace(/\D/g, '').slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2');
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

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') handleBuscar();
  };

  const campos = data
    ? [
        { label: 'CEP', value: data.cep, icon: Hash, col: 'sm:col-span-1' },
        { label: 'Logradouro', value: data.logradouro || '�', icon: MapPin, col: 'sm:col-span-2' },
        { label: 'Complemento', value: data.complemento || '�', icon: Building2, col: 'sm:col-span-1' },
        { label: 'Bairro', value: data.bairro || '�', icon: Map, col: 'sm:col-span-1' },
        { label: 'Cidade', value: data.localidade, icon: Building2, col: 'sm:col-span-1' },
        { label: 'UF', value: data.uf, icon: Map, col: 'sm:col-span-1' },
        { label: 'DDD', value: data.ddd, icon: Phone, col: 'sm:col-span-1' },
        { label: 'IBGE', value: data.ibge, icon: Hash, col: 'sm:col-span-1' },
        { label: 'GIA', value: data.gia || '�', icon: Hash, col: 'sm:col-span-1' },
        { label: 'SIAFI', value: data.siafi, icon: Hash, col: 'sm:col-span-1' },
      ]
    : [];

  return (
    <div className="space-y-8">
      {/* Cabe�alho */}
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-sky-100">
          <MapPin className="h-5 w-5 text-sky-600" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Consulta de CEP</h1>
          <p className="text-sm text-gray-500">
            Consulta externa via{' '}
            <a
              href="https://viacep.com.br"
              target="_blank"
              rel="noopener noreferrer"
              className="text-sky-600 hover:underline"
            >
              ViaCEP
            </a>
          </p>
        </div>
      </div>

      {/* Campo de busca */}
      <Card>
        <CardContent className="py-6">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <div className="flex-1">
              <label htmlFor="cep-input" className="mb-1.5 block text-sm font-medium text-gray-700">
                Digite o CEP
              </label>
              <Input
                id="cep-input"
                placeholder="00000-000"
                value={input}
                onChange={(e) => setInput(formatCep(e.target.value))}
                onKeyDown={handleKeyDown}
                maxLength={9}
                className="font-mono tracking-widest"
              />
            </div>
            <Button
              onClick={handleBuscar}
              disabled={input.replace(/\D/g, '').length !== 8 || isFetching}
              className="flex items-center gap-2"
            >
              {isFetching ? <Spinner className="h-4 w-4" /> : <Search className="h-4 w-4" />}
              Buscar
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Estado de carregamento */}
      {isLoading && (
        <div className="flex items-center justify-center py-12">
          <Spinner className="h-8 w-8 text-sky-600" />
        </div>
      )}

      {/* Erro */}
      {isError && apiError && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-4">
          <p className="text-sm font-medium text-red-700">{apiError.title}</p>
          <p className="mt-1 text-xs text-red-500">Verifique se o CEP informado é válido e tente novamente.</p>
        </div>
      )}

      {/* Resultados */}
      {data && !isLoading && (
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <div className="h-px flex-1 bg-gray-200" />
            <span className="text-xs font-medium uppercase tracking-wider text-gray-400">Endereço encontrado</span>
            <div className="h-px flex-1 bg-gray-200" />
          </div>

          <div className="grid gap-4 sm:grid-cols-3">
            {campos.map(({ label, value, icon: Icon, col }) => (
              <Card key={label} className={col}>
                <CardContent className="flex items-start gap-3 py-4">
                  <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-sky-50">
                    <Icon className="h-4 w-4 text-sky-600" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-400">{label}</p>
                    <p className="truncate text-sm font-semibold text-gray-900">{value}</p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Estado vazio inicial */}
      {!cepBusca && !isLoading && (
        <div className="flex flex-col items-center justify-center gap-3 py-16 text-gray-400">
          <MapPin className="h-12 w-12 opacity-30" />
          <p className="text-sm">Informe um CEP para consultar o endere�o completo.</p>
        </div>
      )}
    </div>
  );
}
