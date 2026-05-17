# Documentação do Backend — Arquitetura de Microserviços .NET 10

> Guia de estudo detalhado que explica cada camada do backend, do recebimento de uma requisição HTTP até a persistência no banco de dados e a troca de mensagens entre serviços. O CRUD de **Produtos** é usado como caso de estudo principal, com o fluxo de **Pedidos** explicando a comunicação entre serviços.

---

## Sumário

1. [Visão Geral da Arquitetura Backend](#1-visão-geral-da-arquitetura-backend)
2. [Estrutura de Projetos](#2-estrutura-de-projetos)
3. [SharedKernel — O Contrato Compartilhado](#3-sharedkernel--o-contrato-compartilhado)
   - 3.1 [Entity — Classe base de entidades](#31-entity--classe-base-de-entidades)
   - 3.2 [ValueObject — Objetos por valor](#32-valueobject--objetos-por-valor)
   - 3.3 [Result Pattern — Sem exceções para fluxos esperados](#33-result-pattern--sem-exceções-para-fluxos-esperados)
   - 3.4 [Error — Erros tipados e identificáveis](#34-error--erros-tipados-e-identificáveis)
   - 3.5 [IDomainEvent e IIntegrationEvent](#35-idomainevent-e-iintegrationevent)
   - 3.6 [DomainException — Exceções de regra de negócio](#36-domainexception--exceções-de-regra-de-negócio)
4. [Camada Domain — Regras de Negócio Puras](#4-camada-domain--regras-de-negócio-puras)
   - 4.1 [Produto — Entidade raiz do serviço de Produtos](#41-produto--entidade-raiz-do-serviço-de-produtos)
   - 4.2 [Pedido e ItemPedido — Agregado de Pedidos](#42-pedido-e-itempedido--agregado-de-pedidos)
   - 4.3 [StatusPedido — Enum de estado](#43-statuspedido--enum-de-estado)
5. [Camada Application — Casos de Uso com CQRS](#5-camada-application--casos-de-uso-com-cqrs)
   - 5.1 [O que é CQRS](#51-o-que-é-cqrs)
   - 5.2 [Commands — Criar Produto (caso de estudo completo)](#52-commands--criar-produto-caso-de-estudo-completo)
   - 5.3 [Queries — Listar e Obter Produto](#53-queries--listar-e-obter-produto)
   - 5.4 [Pipeline de Behaviors do MediatR](#54-pipeline-de-behaviors-do-mediatr)
   - 5.5 [Abstrações — Repository e UnitOfWork](#55-abstrações--repository-e-unitofwork)
   - 5.6 [Registro de Serviços — ApplicationServiceExtensions](#56-registro-de-serviços--applicationserviceextensions)
6. [Camada Infrastructure — Implementação Concreta](#6-camada-infrastructure--implementação-concreta)
   - 6.1 [ProdutosDbContext — O contexto do EF Core](#61-produtosdbcontext--o-contexto-do-ef-core)
   - 6.2 [ProdutoConfiguration — Mapeamento da tabela](#62-produtoconfiguration--mapeamento-da-tabela)
   - 6.3 [ProdutoRepository — Acesso aos dados](#63-produtorepository--acesso-aos-dados)
   - 6.4 [Migrations — Versionamento do banco de dados](#64-migrations--versionamento-do-banco-de-dados)
   - 6.5 [MigrationExtensions — Migrations automáticas no startup](#65-migrationextensions--migrations-automáticas-no-startup)
   - 6.6 [InfrastructureServiceExtensions — Registro do EF e MassTransit](#66-infrastructureserviceextensions--registro-do-ef-e-masstransit)
7. [Camada API — Entrada HTTP](#7-camada-api--entrada-http)
   - 7.1 [Program.cs — Bootstrap da aplicação](#71-programcs--bootstrap-da-aplicação)
   - 7.2 [Minimal APIs — ProdutosEndpoints](#72-minimal-apis--produtosendpoints)
   - 7.3 [ExceptionHandlingMiddleware — Tratamento global de erros](#73-exceptionhandlingmiddleware--tratamento-global-de-erros)
   - 7.4 [Scalar — Interface de teste de API no browser](#74-scalar--interface-de-teste-de-api-no-browser)
8. [Serviço de Pedidos — Comunicação entre Microserviços](#8-serviço-de-pedidos--comunicação-entre-microserviços)
   - 8.1 [CriarPedidoCommandHandler — O fluxo mais complexo](#81-criarpedidocommandhandler--o-fluxo-mais-complexo)
   - 8.2 [IProdutosServiceClient — HTTP síncrono entre serviços](#82-iprodutosserviceclient--http-síncrono-entre-serviços)
   - 8.3 [IEventPublisher e MassTransitEventPublisher](#83-ieventpublisher-e-massttransiteventpublisher)
   - 8.4 [PedidoConfirmadoConsumer — Recebendo o evento em Produtos](#84-pedidoconfirmadoconsumer--recebendo-o-evento-em-produtos)
9. [Fluxo Completo — Do HTTP ao Banco de Dados](#9-fluxo-completo--do-http-ao-banco-de-dados)
   - 9.1 [Criar Produto (CRUD simples)](#91-criar-produto-crud-simples)
   - 9.2 [Criar Pedido (comunicação entre serviços)](#92-criar-pedido-comunicação-entre-serviços)
10. [Infraestrutura Docker](#10-infraestrutura-docker)
11. [Diagrama de Dependências entre Projetos](#11-diagrama-de-dependências-entre-projetos)

---

## 1. Visão Geral da Arquitetura Backend

O backend é composto por dois microserviços independentes que seguem os mesmos princípios arquiteturais. Cada serviço tem quatro camadas internas organizadas em **Clean Architecture**:

```
???????????????????????????????????????????????????????????
?  API  (Produtos.Api / Pedidos.Api)                      ?
?  Minimal APIs, Endpoints, Middleware, Program.cs        ?
?  ? Recebe requisições HTTP, delega ao MediatR           ?
???????????????????????????????????????????????????????????
?  APPLICATION  (Produtos.Application / Pedidos.Appl.)    ?
?  Commands, Queries, Handlers, Validators, Behaviors     ?
?  ? Orquestra casos de uso, sem detalhes de infra        ?
???????????????????????????????????????????????????????????
?  INFRASTRUCTURE  (Produtos.Infra / Pedidos.Infra)       ?
?  DbContext, Repositories, Migrations, RabbitMQ, HTTP    ?
?  ? Implementações concretas das abstrações              ?
???????????????????????????????????????????????????????????
?  DOMAIN  (Produtos.Domain / Pedidos.Domain)             ?
?  Entidades, Enums, Exceções de domínio                  ?
?  ? Regras de negócio puras, zero dependências externas  ?
???????????????????????????????????????????????????????????
             ? todas as camadas dependem desta ?
???????????????????????????????????????????????????????????
?  SHAREDKERNEL  (Arquitetura.SharedKernel)               ?
?  Entity, ValueObject, Result, Error, IIntegrationEvent  ?
?  ? Contratos e primitivos compartilhados entre serviços ?
???????????????????????????????????????????????????????????
```

**Princípio fundamental:** as dependências apontam sempre para dentro. Domain não conhece Application. Application não conhece Infrastructure. A API só conhece Application. Infrastructure implementa interfaces definidas em Application.

---

## 2. Estrutura de Projetos

```
Arquitetura/
??? shared/
?   ??? Arquitetura.SharedKernel/          ? primitivos e contratos compartilhados
?       ??? Common/
?       ?   ??? Error.cs
?       ?   ??? Result.cs
?       ??? Primitives/
?       ?   ??? Entity.cs
?       ?   ??? ValueObject.cs
?       ?   ??? IDomainEvent.cs
?       ??? Messaging/
?       ?   ??? IIntegrationEvent.cs
?       ?   ??? PedidoConfirmadoIntegrationEvent.cs
?       ??? Exceptions/
?           ??? DomainException.cs
?
??? services/
?   ??? Produtos/
?   ?   ??? Produtos.Domain/               ? entidade Produto, exceções
?   ?   ??? Produtos.Application/          ? commands, queries, behaviors
?   ?   ??? Produtos.Infrastructure/       ? EF Core, migrations, RabbitMQ consumer
?   ?   ??? Produtos.Api/                  ? endpoints, middleware, Program.cs
?   ?
?   ??? Pedidos/
?       ??? Pedidos.Domain/                ? entidades Pedido/ItemPedido, enum status
?       ??? Pedidos.Application/           ? criar pedido, listar, abstrações HTTP
?       ??? Pedidos.Infrastructure/        ? EF Core, HTTP client, RabbitMQ publisher
?       ??? Pedidos.Api/                   ? endpoints, middleware, Program.cs
?
??? docker-compose.yml                     ? orquestração de containers
??? Arquitetura.Server/
    ??? BACKEND.md                         ? este arquivo
```

---

## 3. SharedKernel — O Contrato Compartilhado

```
shared/Arquitetura.SharedKernel/
```

O SharedKernel é uma biblioteca sem dependências de framework (somente .NET base). Ele garante que os dois microserviços falem a mesma "língua" ao trocar mensagens e ao implementar padrões comuns.

### 3.1 Entity — Classe base de entidades

```csharp
// shared/Arquitetura.SharedKernel/Primitives/Entity.cs

public abstract class Entity
{
    // Lista privada — só a própria entidade pode adicionar eventos
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;
    protected Entity() { }  // construtor vazio necessário para o EF Core

    public Guid Id { get; private init; }

    // Expõe a lista como somente-leitura para o mundo externo
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Método protegido — só subclasses podem emitir eventos
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    // Chamado pela Infrastructure após persistir os eventos
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**O que `Guid Id { get; private init; }` significa?**
- `private init` = pode ser definido apenas no construtor (jamais alterado depois)
- Garante imutabilidade da identidade da entidade

**Por que `IReadOnlyList` ao invés de `List`?**
- Expõe os eventos sem permitir que código externo adicione ou remova itens diretamente
- Encapsulamento: a entidade controla os próprios eventos

### 3.2 ValueObject — Objetos por valor

```csharp
// shared/Arquitetura.SharedKernel/Primitives/ValueObject.cs

public abstract class ValueObject : IEquatable<ValueObject>
{
    // Subclasses retornam os componentes que definem a igualdade
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other) =>
        other is not null && GetAtomicValues().SequenceEqual(other.GetAtomicValues());

    public override int GetHashCode() =>
        GetAtomicValues()
            .Aggregate(0, (hash, value) =>
                HashCode.Combine(hash, value?.GetHashCode() ?? 0));

    // Operadores == e != sobrecarregados para comparação natural
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;
}
```

**Diferença entre Entidade e Value Object:**

| | Entidade | Value Object |
|--|---------|-------------|
| Identidade | Tem `Id` único (Guid) | Não tem Id |
| Igualdade | Por Id | Por valores dos atributos |
| Exemplo | `Produto`, `Pedido` | `Endereço`, `Dinheiro` |
| Mutabilidade | Pode mudar estado | Imutável por natureza |

`ItemPedido` no projeto usa `record` do C# — o compilador gera automaticamente igualdade por valor, equivalente a implementar `ValueObject`.

### 3.3 Result Pattern — Sem exceções para fluxos esperados

```csharp
// shared/Arquitetura.SharedKernel/Common/Result.cs

public class Result
{
    // Construtor protegido — só instanciado via métodos estáticos
    protected Result(bool isSuccess, Error error)
    {
        // Invariantes: não pode ter sucesso com erro, nem falha sem erro
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("...");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("...");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // Métodos fábrica — a única forma de criar um Result
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    // Value lança exceção se acessado em resultado de falha — proteção explícita
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado de falha.");
}
```

**Por que usar Result Pattern em vez de lançar exceções?**

```csharp
// ? Sem Result Pattern — exceções para fluxos esperados (caro e semânticamente errado)
public Produto ObterPorId(Guid id)
{
    var produto = _repo.ObterPorId(id);
    if (produto is null) throw new NotFoundException("Produto não encontrado");
    return produto;
}

// ? Com Result Pattern — falha é parte normal do fluxo
public Result<Produto> ObterPorId(Guid id)
{
    var produto = _repo.ObterPorId(id);
    if (produto is null) return Result.Failure<Produto>(Error.NaoEncontrado);
    return Result.Success(produto);
}

// No endpoint — o chamador decide o que fazer com o resultado
var result = await sender.Send(new ObterProdutoPorIdQuery(id), ct);
return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
```

Exceções devem representar situações **inesperadas** (banco caiu, timeout de rede). "Produto não encontrado" é algo esperado e deve ser tratado como dado, não como exceção.

### 3.4 Error — Erros tipados e identificáveis

```csharp
// shared/Arquitetura.SharedKernel/Common/Error.cs

// record = igualdade por valor; sealed = não pode ser herdado
public sealed record Error(string Code, string Description)
{
    // Sentinela para "sem erro" — evita null
    public static readonly Error None = new(string.Empty, string.Empty);

    // Erros genéricos reutilizáveis em qualquer serviço
    public static readonly Error NaoEncontrado = new("Geral.NaoEncontrado", "...");
    public static readonly Error Invalido = new("Geral.Invalido", "...");
    public static readonly Error Conflito = new("Geral.Conflito", "...");
}
```

O `Code` permite que o cliente (frontend ou outro serviço) identifique programaticamente o tipo de erro sem fazer parsing de mensagens de texto.

### 3.5 IDomainEvent e IIntegrationEvent

```csharp
// IDomainEvent — marcador para eventos DENTRO do mesmo serviço
public interface IDomainEvent;
// "interface sem membros" = marcador/tag — apenas para tipagem

// IIntegrationEvent — contrato para eventos ENTRE serviços via broker
public interface IIntegrationEvent
{
    Guid EventId { get; }       // idempotência: permite detectar duplicatas
    DateTime OcorridoEm { get; } // rastreabilidade temporal
}
```

**Diferença fundamental:**

| | Domain Event | Integration Event |
|--|-------------|------------------|
| Escopo | Intra-serviço | Inter-serviços |
| Transporte | MediatR (in-process) | RabbitMQ (rede) |
| Falha | Transacional | Eventual (at-least-once) |
| Exemplo | `ProdutoCriadoEvent` | `PedidoConfirmadoIntegrationEvent` |

```csharp
// O evento de integração compartilhado entre Pedidos (publica) e Produtos (consome)
public sealed record PedidoConfirmadoIntegrationEvent(
    Guid EventId,
    DateTime OcorridoEm,
    Guid PedidoId,
    Guid ClienteId,
    IReadOnlyList<ItemPedidoConfirmado> Itens) : IIntegrationEvent;

public sealed record ItemPedidoConfirmado(Guid ProdutoId, int Quantidade);
```

### 3.6 DomainException — Exceções de regra de negócio

```csharp
// Base abstrata — não pode ser instanciada diretamente
public abstract class DomainException(string message) : Exception(message);

// Exceções específicas de cada serviço herdam dela
public sealed class ProdutoException(string message) : DomainException(message);
public sealed class ProdutoNaoEncontradoException(Guid id)
    : DomainException($"Produto com Id '{id}' não foi encontrado.");
```

O `ExceptionHandlingMiddleware` captura `DomainException` e retorna `400 Bad Request`, separando-a de `ValidationException` (422) e exceções genéricas (500).

---

## 4. Camada Domain — Regras de Negócio Puras

```
services/Produtos/Produtos.Domain/
services/Pedidos/Pedidos.Domain/
```

O Domain é a camada mais interna e mais importante. **Não tem dependências de NuGet além do SharedKernel.** Contém apenas C# puro e regras de negócio.

### 4.1 Produto — Entidade raiz do serviço de Produtos

```csharp
// services/Produtos/Produtos.Domain/Entities/Produto.cs

public sealed class Produto : Entity
{
    // Construtor privado vazio — exclusivo para o EF Core reconstruir do banco
    // O EF Core usa reflexão para instanciar sem chamar construtores públicos
    private Produto() { }

    // Construtor de negócio — privado, só chamado pelo método fábrica Criar()
    private Produto(Guid id, string nome, string descricao, decimal preco, int estoque)
        : base(id)  // repassa o Id para Entity
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
        CriadoEm = DateTime.UtcNow;  // UTC sempre — sem ambiguidade de fuso
    }

    // Propriedades com setter privado — só o próprio Produto altera seus dados
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }  // nullable — pode nunca ter sido atualizado

    // ?? MÉTODO FÁBRICA ??????????????????????????????????????????????????????
    // Único ponto de entrada para criar um Produto
    // Centraliza TODAS as validações de criação em um lugar
    public static Produto Criar(string nome, string descricao, decimal preco, int estoque)
    {
        // Guard clauses — validam invariantes de domínio antes de criar
        if (string.IsNullOrWhiteSpace(nome))
            throw new ProdutoException("O nome do produto é obrigatório.");
        if (preco <= 0)
            throw new ProdutoException("O preço deve ser maior que zero.");
        if (estoque < 0)
            throw new ProdutoException("O estoque não pode ser negativo.");

        // nome.Trim() — remove espaços acidentais antes de persistir
        return new Produto(Guid.NewGuid(), nome.Trim(), descricao.Trim(), preco, estoque);
    }

    // ?? COMPORTAMENTOS ??????????????????????????????????????????????????????
    public void Atualizar(string nome, string descricao, decimal preco, int estoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ProdutoException("O nome do produto é obrigatório.");
        if (preco <= 0)
            throw new ProdutoException("O preço deve ser maior que zero.");
        if (estoque < 0)
            throw new ProdutoException("O estoque não pode ser negativo.");

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
        Estoque = estoque;
        AtualizadoEm = DateTime.UtcNow;  // marca a data da última alteração
    }

    // Chamado pelo consumer do RabbitMQ quando um pedido é confirmado
    public void DebitarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ProdutoException("A quantidade a debitar deve ser maior que zero.");
        if (Estoque < quantidade)
            throw new ProdutoException(
                $"Estoque insuficiente para '{Nome}'. Disponível: {Estoque}, solicitado: {quantidade}.");

        Estoque -= quantidade;
        AtualizadoEm = DateTime.UtcNow;
    }
}
```

**Padrões aplicados:**
- **Tell, Don't Ask**: em vez de ler `produto.Estoque` e subtrair externamente, pedimos `produto.DebitarEstoque(n)` — a entidade executa e valida
- **Factory Method** (`Criar`): garante que nenhum `Produto` inválido seja criado
- **Encapsulamento total**: `private set` em todas as propriedades

### 4.2 Pedido e ItemPedido — Agregado de Pedidos

```csharp
// services/Pedidos/Pedidos.Domain/Entities/Pedido.cs

public sealed class Pedido : Entity
{
    // Lista interna privada — encapsulamento dos itens
    private readonly List<ItemPedido> _itens = [];

    private Pedido(Guid id, Guid clienteId) : base(id)
    {
        ClienteId = clienteId;
        Status = StatusPedido.Pendente;  // todo pedido começa Pendente
        CriadoEm = DateTime.UtcNow;
    }

    public Guid ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    // Expõe itens como somente-leitura
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    // Propriedade calculada — não persiste no banco, calculada em memória
    public decimal Total => _itens.Sum(i => i.PrecoUnitario * i.Quantidade);

    public static Pedido Criar(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
            throw new PedidoException("O clienteId é obrigatório.");
        return new Pedido(Guid.NewGuid(), clienteId);
    }

    public void AdicionarItem(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        // Guarda de estado — itens só podem ser adicionados em pedidos Pendentes
        if (Status != StatusPedido.Pendente)
            throw new PedidoException("Só é possível adicionar itens em pedidos pendentes.");

        // Idempotência: se o produto já existe, soma a quantidade
        var itemExistente = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente is not null)
        {
            _itens.Remove(itemExistente);
            // record with expression — cria novo record com Quantidade somada
            _itens.Add(itemExistente with { Quantidade = itemExistente.Quantidade + quantidade });
        }
        else
        {
            _itens.Add(new ItemPedido(Guid.NewGuid(), Id, produtoId, nomeProduto, precoUnitario, quantidade));
        }

        AtualizadoEm = DateTime.UtcNow;
    }

    public void Confirmar()
    {
        if (Status != StatusPedido.Pendente)
            throw new PedidoException("Apenas pedidos pendentes podem ser confirmados.");
        if (_itens.Count == 0)
            throw new PedidoException("O pedido deve ter ao menos um item.");

        Status = StatusPedido.Confirmado;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status is StatusPedido.Entregue or StatusPedido.Cancelado)
            throw new PedidoException("Pedido já foi entregue ou cancelado.");

        Status = StatusPedido.Cancelado;
        AtualizadoEm = DateTime.UtcNow;
    }
}
```

```csharp
// services/Pedidos/Pedidos.Domain/Entities/ItemPedido.cs

// record = igualdade por valor + imutabilidade por padrão
// sealed record = não pode ser herdado
public sealed record ItemPedido(
    Guid Id,
    Guid PedidoId,
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoUnitario,
    int Quantidade);

// ItemPedido é um "owned entity" no EF Core —
// pertence ao Pedido e não tem DbSet próprio
```

**`record` vs `class` para ItemPedido:**

| | class | record |
|--|-------|--------|
| Igualdade | Por referência (`==` compara endereço de memória) | Por valor (`==` compara todos os campos) |
| Imutabilidade | Opt-in (`readonly`, `init`) | Padrão (todos os campos são `init`) |
| `with` expression | Não | Sim — cria cópia com campo alterado |
| Uso ideal | Entidades com identidade | DTOs, Value Objects, eventos |

### 4.3 StatusPedido — Enum de estado

```csharp
public enum StatusPedido
{
    Pendente   = 0,  // estado inicial — aguardando confirmação
    Confirmado = 1,  // pedido aceito — trigger para debitar estoque
    Cancelado  = 2,  // pedido cancelado
    Entregue   = 3   // pedido entregue ao cliente
}
```

**Valores explícitos (0, 1, 2, 3):** mesmo que a ordem mude no código, os valores persistidos no banco continuam corretos. Sem valores explícitos, reordenar o enum corromperia dados existentes.

A transição de estados é protegida pelos métodos `Confirmar()` e `Cancelar()` — a entidade garante que transições inválidas não aconteçam.

---

## 5. Camada Application — Casos de Uso com CQRS

```
services/Produtos/Produtos.Application/
```

### 5.1 O que é CQRS

**CQRS (Command Query Responsibility Segregation)** separa operações de escrita (Commands) de operações de leitura (Queries):

```
Leitura  ? Query   ? QueryHandler   ? Retorna dados (sem efeitos colaterais)
Escrita  ? Command ? CommandHandler ? Modifica estado (sem retornar dados desnecessários)
```

O **MediatR** atua como mediador — o código que quer executar algo não conhece quem vai processar. Envia a mensagem e aguarda o resultado:

```csharp
// O Endpoint conhece apenas o Command — não conhece o Handler
var result = await sender.Send(new CriarProdutoCommand(nome, desc, preco, estoque), ct);
//                                                ?
//                              MediatR encontra CriarProdutoCommandHandler e executa
```

### 5.2 Commands — Criar Produto (caso de estudo completo)

**Passo 1: O Command**

```csharp
// Features/Commands/CriarProduto/CriarProdutoCommand.cs

// record = imutável — os dados do command não mudam após criação
// IRequest<Result<Guid>> = este command retorna Result<Guid> quando processado
public sealed record CriarProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result<Guid>>;
```

Um Command é um DTO imutável que carrega a intenção de negócio. Não tem lógica — é apenas dados.

**Passo 2: O Validator**

```csharp
// Features/Commands/CriarProduto/CriarProdutoCommandValidator.cs

internal sealed class CriarProdutoCommandValidator : AbstractValidator<CriarProdutoCommand>
{
    public CriarProdutoCommandValidator()
    {
        // FluentValidation — DSL fluente para regras de validação
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome não pode ter mais de 200 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0).WithMessage("O estoque não pode ser negativo.");
    }
}
```

**Por que validar no Application se o Domain também valida?**
- O **Validator** valida a _forma_ da requisição (campos obrigatórios, tamanho máximo) — executado antes de chegar ao Domain
- O **Domain** valida as _invariantes de negócio_ (preço > 0, estoque >= 0) — segunda linha de defesa
- São defesas complementares: o Validator para antes de instanciar objetos de domínio

**Passo 3: O Handler**

```csharp
// Features/Commands/CriarProduto/CriarProdutoCommandHandler.cs

// internal sealed — só acessível dentro da assembly Application
// Primary constructor — injeção de dependência declarativa (C# 12+)
internal sealed class CriarProdutoCommandHandler(
    IProdutoRepository repository,  // abstração — não conhece SQL
    IUnitOfWork unitOfWork)         // abstração — não conhece DbContext
    : IRequestHandler<CriarProdutoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CriarProdutoCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Delega a criação ao Domain (que aplica as invariantes)
        var produto = Produto.Criar(
            request.Nome, request.Descricao, request.Preco, request.Estoque);

        // 2. Persiste via abstração (Repository Pattern)
        await repository.AdicionarAsync(produto, cancellationToken);

        // 3. Confirma a transação (Unit of Work Pattern)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Retorna o Id do novo produto encapsulado em Result
        return Result.Success(produto.Id);
    }
}
```

**Sequência de execução — por que essa ordem?**
1. `Produto.Criar(...)` — valida e constrói em memória. Se falhar, nada foi enviado ao banco
2. `repository.AdicionarAsync(...)` — enfileira o INSERT no change tracker do EF (ainda sem SQL)
3. `unitOfWork.SaveChangesAsync(...)` — executa `SaveChangesAsync()` no DbContext, que gera e envia o SQL em uma transação

### 5.3 Queries — Listar e Obter Produto

```csharp
// Features/Queries/ListarProdutos/ListarProdutosQuery.cs
// Query sem parâmetros — lista tudo
public sealed record ListarProdutosQuery : IRequest<Result<IReadOnlyList<Produto>>>;
```

```csharp
// Features/Queries/ListarProdutos/ListarProdutosQueryHandler.cs
internal sealed class ListarProdutosQueryHandler(IProdutoRepository repository)
    : IRequestHandler<ListarProdutosQuery, Result<IReadOnlyList<Produto>>>
{
    public async Task<Result<IReadOnlyList<Produto>>> Handle(
        ListarProdutosQuery request, CancellationToken cancellationToken)
    {
        var produtos = await repository.ListarAsync(cancellationToken);
        return Result.Success(produtos);
        // Queries raramente retornam Result.Failure — a lista vazia é sucesso
    }
}
```

```csharp
// Features/Queries/ObterProdutoPorId/ObterProdutoPorIdQuery.cs
// Query com parâmetro — busca pelo Id
public sealed record ObterProdutoPorIdQuery(Guid Id) : IRequest<Result<Produto>>;
```

### 5.4 Pipeline de Behaviors do MediatR

O MediatR suporta um pipeline de **Behaviors** — middleware que envolve todos os Handlers. A ordem de registro em `AddApplicationServices` determina a ordem de execução:

```
Requisição HTTP
      ?
      ?
  LoggingBehavior    ? registrado primeiro = mais externo
      ?
      ?
  ValidationBehavior ? registrado segundo = executa antes do Handler
      ?
      ?
  CommandHandler / QueryHandler  ? o código de negócio real
      ?
      ?  (resposta sobe o pipeline na ordem inversa)
  ValidationBehavior
      ?
      ?
  LoggingBehavior
      ?
      ?
  Resposta para o Endpoint
```

**LoggingBehavior:**

```csharp
internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,  // próximo na pipeline
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processando {RequestName}", typeof(TRequest).Name);

        var response = await next(cancellationToken);  // executa o restante da pipeline

        logger.LogInformation("Concluído {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

**ValidationBehavior:**

```csharp
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)  // todos os validators do request injetados
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se não há validators registrados para este request, pula a validação
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        // Executa todos os validators e coleta todos os erros
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)   // "achata" a lista de listas
            .Where(f => f is not null)
            .ToList();

        // Se há falhas, lança exceção — capturada pelo ExceptionHandlingMiddleware
        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);  // validação passou — continua
    }
}
```

### 5.5 Abstrações — Repository e UnitOfWork

```csharp
// Abstractions/IProdutoRepository.cs
// Interface definida em Application — implementada em Infrastructure
// Application depende desta interface, nunca da implementação concreta
public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Produto produto, CancellationToken ct = default);
    void Atualizar(Produto produto);  // síncrono — EF Core já rastreia o objeto
    void Remover(Produto produto);    // síncrono — idem
}

// Abstractions/IUnitOfWork.cs
// Abstrai o SaveChangesAsync do DbContext
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

**Por que `Atualizar` e `Remover` são síncronos?**
O EF Core usa um Change Tracker que rastreia objetos carregados. `Update()` e `Remove()` apenas marcam o estado no tracker (operação em memória, instantânea). O SQL só é gerado quando `SaveChangesAsync()` é chamado.

### 5.6 Registro de Serviços — ApplicationServiceExtensions

```csharp
// ApplicationServiceExtensions.cs

public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    var assembly = typeof(ApplicationServiceExtensions).Assembly;

    // Registra todos os Handlers, Commands e Queries do assembly
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

    // Registra os Behaviors na ordem correta (Logging primeiro, depois Validation)
    // AddTransient = nova instância por requisição ao MediatR
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Registra todos os AbstractValidator<T> do assembly automaticamente
    services.AddValidatorsFromAssembly(assembly);

    return services;
}
```

**Extension Methods no `IServiceCollection`** são o padrão .NET para organizar o registro de serviços por responsabilidade. Cada camada registra os próprios serviços, e o `Program.cs` apenas os combina:

```csharp
builder.Services.AddApplicationServices();       // Application layer
builder.Services.AddInfrastructureServices(...); // Infrastructure layer
```

---

## 6. Camada Infrastructure — Implementação Concreta

```
services/Produtos/Produtos.Infrastructure/
```

### 6.1 ProdutosDbContext — O contexto do EF Core

```csharp
// Data/ProdutosDbContext.cs

// Primary constructor recebe DbContextOptions — injetado pelo DI
// Implementa IUnitOfWork — o próprio DbContext é a unidade de trabalho
public sealed class ProdutosDbContext(DbContextOptions<ProdutosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    // DbSet<Produto> = representa a tabela "Produtos" no banco
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> do assembly automaticamente
        // Convenção: encontra ProdutoConfiguration e aplica
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

**Por que `IUnitOfWork` é implementado pelo `DbContext`?**
O `DbContext` do EF Core já é uma Unit of Work nativa — ele rastreia mudanças e as persiste atomicamente. Em vez de criar uma classe `UnitOfWork` extra que apenas delega para o `DbContext`, registramos o próprio `DbContext` como `IUnitOfWork`:

```csharp
// No InfrastructureServiceExtensions:
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProdutosDbContext>());
// Quando Application pede IUnitOfWork, recebe a mesma instância do DbContext
// (scoped = uma instância por requisição HTTP)
```

### 6.2 ProdutoConfiguration — Mapeamento da tabela

```csharp
// Data/Configurations/ProdutoConfiguration.cs

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);          // chave primária

        builder.Property(p => p.Nome)
            .IsRequired()                   // NOT NULL no SQL
            .HasMaxLength(200);             // VARCHAR(200) — alinhado com o Validator

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.Preco)
            .HasPrecision(18, 2);           // DECIMAL(18,2) — importante para dinheiro!
            // Sem HasPrecision, EF gera FLOAT que tem erro de arredondamento

        builder.Property(p => p.CriadoEm)
            .IsRequired();
    }
}
```

**Por que `HasPrecision(18, 2)` é crítico para valores monetários?**
- `FLOAT` e `DOUBLE` são representações binárias — `0.1 + 0.2 = 0.30000000000000004`
- `DECIMAL(18, 2)` é armazenado como inteiro com escala — `0.1 + 0.2 = 0.30` exato
- Nunca use `float/double` para dinheiro; sempre `decimal` no C# e `DECIMAL` no SQL

### 6.3 ProdutoRepository — Acesso aos dados

```csharp
// Repositories/ProdutoRepository.cs

internal sealed class ProdutoRepository(ProdutosDbContext context) : IProdutoRepository
{
    // FirstOrDefaultAsync — retorna null se não encontrar (sem exceção)
    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);

    // AsNoTracking() — EF não rastreia as entidades retornadas
    // Para queries de leitura: 30% mais rápido, menos memória
    // Não usar em entidades que serão modificadas
    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default) =>
        await context.Produtos.AsNoTracking().ToListAsync(ct);

    // AddAsync — enfileira o INSERT no change tracker (sem SQL ainda)
    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default) =>
        await context.Produtos.AddAsync(produto, ct);

    // Update — marca o objeto como modificado no change tracker
    // O EF detecta automaticamente quais colunas mudaram
    public void Atualizar(Produto produto) =>
        context.Produtos.Update(produto);

    // Remove — marca o objeto para deleção no change tracker
    public void Remover(Produto produto) =>
        context.Produtos.Remove(produto);
}
```

**`AsNoTracking()` — quando usar e quando não usar:**

| Operação | AsNoTracking? | Motivo |
|----------|--------------|--------|
| Listar para exibição | ? Sim | Não vai modificar — economiza memória |
| Buscar para editar | ? Não | Precisa rastrear mudanças para gerar UPDATE |
| Buscar para deletar | ? Não | Precisa estar rastreado para gerar DELETE |

### 6.4 Migrations — Versionamento do banco de dados

As migrations do EF Core registram a evolução do schema do banco como código C#. Cada migration tem dois métodos:

```csharp
// Data/Migrations/20260516024428_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQL executado ao APLICAR a migration (upgrade)
        migrationBuilder.CreateTable(
            name: "Produtos",
            columns: table => new {
                Id = table.Column<Guid>(...),
                Nome = table.Column<string>(maxLength: 200, nullable: false),
                Preco = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                // ...
            },
            constraints: table => table.PrimaryKey("PK_Produtos", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // SQL executado ao REVERTER a migration (rollback)
        migrationBuilder.DropTable(name: "Produtos");
    }
}
```

**Comandos úteis de migration:**
```bash
# Criar nova migration após mudar o modelo
dotnet ef migrations add NomeDaMigration --project Produtos.Infrastructure --startup-project Produtos.Api

# Aplicar migrations pendentes manualmente
dotnet ef database update --project Produtos.Infrastructure --startup-project Produtos.Api

# Gerar script SQL (para revisar antes de aplicar em produção)
dotnet ef migrations script --project Produtos.Infrastructure --startup-project Produtos.Api
```

### 6.5 MigrationExtensions — Migrations automáticas no startup

```csharp
// MigrationExtensions.cs

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        // CreateScope() — cria um escopo DI para resolver serviços scoped
        // Necessário porque IServiceProvider no Program.cs é o root (singleton)
        // Não pode resolver serviços scoped diretamente do root provider
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ProdutosDbContext>();

        // MigrateAsync() aplica todas as migrations pendentes automaticamente
        // Idempotente: migrations já aplicadas são ignoradas
        await db.Database.MigrateAsync();
    }
}
```

Chamado em `Program.cs` antes de `app.Run()`:
```csharp
await app.Services.ApplyMigrationsAsync();
// Garante que o banco está sempre atualizado ao iniciar o container
```

### 6.6 InfrastructureServiceExtensions — Registro do EF e MassTransit

```csharp
// InfrastructureServiceExtensions.cs

public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ?? EF CORE ?????????????????????????????????????????????????????????????
    services.AddDbContext<ProdutosDbContext>(options =>
        options.UseSqlServer(
            configuration.GetConnectionString("ProdutosConnection")));
    // AddDbContext = escopo por requisição HTTP (Scoped lifetime)
    // Connection string lida do appsettings.json ou variáveis de ambiente

    services.AddScoped<IProdutoRepository, ProdutoRepository>();
    services.AddScoped<IUnitOfWork>(sp =>
        sp.GetRequiredService<ProdutosDbContext>());
    // IUnitOfWork resolve para a mesma instância de ProdutosDbContext do escopo atual

    // ?? MASSTRANSIT / RABBITMQ ???????????????????????????????????????????????
    services.AddMassTransit(x =>
    {
        // Registra o Consumer que processa PedidoConfirmadoIntegrationEvent
        x.AddConsumer<PedidoConfirmadoConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(configuration["RabbitMQ:Password"] ?? "guest");
            });

            // ConfigureEndpoints: cria automaticamente as filas baseadas nos consumers
            // PedidoConfirmadoConsumer ? fila "pedido-confirmado-integration-event"
            cfg.ConfigureEndpoints(ctx);
        });
    });

    return services;
}
```

---

## 7. Camada API — Entrada HTTP

```
services/Produtos/Produtos.Api/
```

### 7.1 Program.cs — Bootstrap da aplicação

```csharp
// services/Produtos/Produtos.Api/Program.cs

var builder = WebApplication.CreateBuilder(args);

// ?? REGISTRO DE SERVIÇOS ????????????????????????????????????????????????????
builder.Services.AddApplicationServices();           // MediatR, Validators, Behaviors
builder.Services.AddInfrastructureServices(builder.Configuration); // EF Core, RabbitMQ
builder.Services.AddOpenApi();                       // geração do OpenAPI (swagger.json)
builder.Services.AddHealthChecks();                  // endpoint /health

var app = builder.Build();

// ?? MIGRATIONS AUTOMÁTICAS ??????????????????????????????????????????????????
await app.Services.ApplyMigrationsAsync();
// Executa ANTES de qualquer requisição — banco sempre atualizado

// ?? MIDDLEWARE PIPELINE ?????????????????????????????????????????????????????
// Ordem importa: cada middleware envolve os seguintes
app.UseMiddleware<ExceptionHandlingMiddleware>();  // captura exceções globalmente

// ?? ENDPOINTS ???????????????????????????????????????????????????????????????
app.MapOpenApi();                                       // GET /openapi/v1.json
app.MapScalarApiReference(opt =>
    opt.WithTitle("Produtos API"));                     // GET /scalar/v1 (UI interativa)

app.MapProdutosEndpoints();                            // rotas de negócio
app.MapHealthChecks("/health");                        // GET /health ? {"status":"Healthy"}

app.Run();
```

**A ordem do middleware é crítica.** O `ExceptionHandlingMiddleware` deve ser registrado antes dos endpoints para capturar exceções lançadas por qualquer rota.

### 7.2 Minimal APIs — ProdutosEndpoints

```csharp
// Endpoints/ProdutosEndpoints.cs

public static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup = prefixo de rota + metadados compartilhados para o grupo
        var group = app.MapGroup("/api/produtos").WithTags("Produtos");
        // WithTags agrupa os endpoints no Scalar/Swagger

        group.MapGet("/", ListarAsync).WithSummary("Lista todos os produtos");
        group.MapGet("/{id:guid}", ObterPorIdAsync)
             .WithName("ObterProduto")    // nome usado em CreatedAtRoute
             .WithSummary("Obtém produto por Id");
        group.MapPost("/", CriarAsync).WithSummary("Cria um novo produto");
        group.MapPut("/{id:guid}", AtualizarAsync).WithSummary("Atualiza um produto");
        group.MapDelete("/{id:guid}", DeletarAsync).WithSummary("Remove um produto");

        return app;
    }

    // ISender = interface do MediatR para enviar Commands e Queries
    // CancellationToken ct = cancelamento automático se o cliente desconectar
    private static async Task<IResult> ListarAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListarProdutosQuery(), ct);
        return Results.Ok(result.Value);
        // Results.Ok(value) ? 200 OK + body JSON serializado
    }

    private static async Task<IResult> ObterPorIdAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ObterProdutoPorIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)   // 200 OK + produto
            : Results.NotFound();        // 404 Not Found
    }

    private static async Task<IResult> CriarAsync(
        CriarProdutoCommand command,  // ASP.NET Core desserializa o body JSON automaticamente
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.CreatedAtRoute(
                "ObterProduto",                    // nome da rota de destino
                new { id = result.Value },         // parâmetros da rota
                result.Value)                      // body da resposta (Guid do novo produto)
              // ? 201 Created + Location: /api/produtos/{id} + body: {guid}
            : Results.BadRequest(result.Error);    // 400 Bad Request + detalhes do erro
    }

    private static async Task<IResult> AtualizarAsync(
        Guid id,
        AtualizarProdutoRequest request,   // record desserializado do body JSON
        ISender sender,
        CancellationToken ct)
    {
        // Combina o id da rota com os dados do body em um único Command
        var result = await sender.Send(
            new AtualizarProdutoCommand(id, request.Nome, request.Descricao, request.Preco, request.Estoque),
            ct);
        return result.IsSuccess
            ? Results.NoContent()   // 204 No Content — atualizado, sem corpo
            : Results.NotFound();   // 404 se o produto não existir
    }

    private static async Task<IResult> DeletarAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeletarProdutoCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()   // 204 No Content — deletado
            : Results.NotFound();   // 404 se não existir
    }
}

// Record local — apenas para desserialização do body do PUT
public sealed record AtualizarProdutoRequest(string Nome, string Descricao, decimal Preco, int Estoque);
```

**Tabela de status HTTP usados:**

| Situação | Status | Método |
|----------|--------|--------|
| Listagem / busca com resultado | 200 OK | `Results.Ok(value)` |
| Recurso criado | 201 Created | `Results.CreatedAtRoute(...)` |
| Atualização / deleção concluída | 204 No Content | `Results.NoContent()` |
| Recurso não encontrado | 404 Not Found | `Results.NotFound()` |
| Dados inválidos (Result.Failure) | 400 Bad Request | `Results.BadRequest(error)` |
| Validação FluentValidation | 422 Unprocessable | Middleware |
| Regra de negócio violada | 400 Bad Request | Middleware |
| Erro inesperado | 500 Internal Server Error | Middleware |

### 7.3 ExceptionHandlingMiddleware — Tratamento global de erros

```csharp
// Middleware/ExceptionHandlingMiddleware.cs

internal sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,    // próximo middleware na pipeline
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);  // executa toda a pipeline de endpoints
        }
        catch (ValidationException ex)  // lançada pelo ValidationBehavior
        {
            logger.LogWarning("Erro de validação: {Errors}", ex.Errors);
            await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity,
                "Erro de Validação",
                ex.Errors.Select(e => e.ErrorMessage));
            // 422 Unprocessable Entity
        }
        catch (DomainException ex)  // lançada pelas entidades de domínio
        {
            logger.LogWarning("Regra de negócio violada: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.BadRequest,
                "Regra de Negócio", [ex.Message]);
            // 400 Bad Request
        }
        catch (Exception ex)  // qualquer outra exceção inesperada
        {
            logger.LogError(ex, "Erro inesperado");
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                "Erro Interno",
                ["Ocorreu um erro inesperado. Tente novamente mais tarde."]);
            // 500 Internal Server Error — mensagem genérica (não expõe detalhes internos)
        }
    }

    private static Task WriteResponseAsync(
        HttpContext context, HttpStatusCode status, string title, IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/problem+json";  // RFC 7807
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            errors  // array de mensagens de erro
        }));
    }
}
```

**Exemplo de resposta de erro de validação:**
```json
{
    "type": "https://httpstatuses.com/422",
    "title": "Erro de Validação",
    "status": 422,
    "errors": [
        "O nome é obrigatório.",
        "O preço deve ser maior que zero."
    ]
}
```

### 7.4 Scalar — Interface de teste de API no browser

O **Scalar** substitui o Swagger UI com uma interface mais moderna. Acessível em:
- Produtos API: `http://localhost:5001/scalar/v1`
- Pedidos API: `http://localhost:5002/scalar/v1`

```csharp
app.MapOpenApi();                         // Gera GET /openapi/v1.json
app.MapScalarApiReference(opt =>          // Serve a UI que consome o JSON acima
    opt.WithTitle("Produtos API"));       // ? GET /scalar/v1
```

O `MapOpenApi()` do ASP.NET Core 10 gera automaticamente o contrato OpenAPI 3.x a partir dos endpoints registrados com `.WithSummary()`, `.WithTags()` e tipos de parâmetro/retorno inferidos.

---

## 8. Serviço de Pedidos — Comunicação entre Microserviços

O serviço de Pedidos demonstra dois padrões de comunicação entre microserviços:
1. **Síncrono (HTTP)**: Pedidos consulta Produtos para validar preço e estoque antes de criar o pedido
2. **Assíncrono (RabbitMQ)**: Após confirmar o pedido, Pedidos publica um evento que Produtos consome para debitar o estoque

### 8.1 CriarPedidoCommandHandler — O fluxo mais complexo

```csharp
// services/Pedidos/Pedidos.Application/Features/Commands/CriarPedido/CriarPedidoCommandHandler.cs

public sealed record CriarPedidoCommand(
    Guid ClienteId,
    List<ItemPedidoDto> Itens) : IRequest<Result<Guid>>;

public sealed record ItemPedidoDto(Guid ProdutoId, int Quantidade);

internal sealed class CriarPedidoCommandHandler(
    IPedidoRepository pedidoRepository,
    IProdutosServiceClient produtosClient,  // ? HTTP client para Produtos API
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)          // ? publisher RabbitMQ
    : IRequestHandler<CriarPedidoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        // 1. Cria o agregado Pedido em estado Pendente
        var pedido = Pedido.Criar(request.ClienteId);

        // 2. Para cada item solicitado, consulta o serviço de Produtos via HTTP
        foreach (var item in request.Itens)
        {
            var produto = await produtosClient.ObterProdutoAsync(
                item.ProdutoId, cancellationToken);

            // Valida se o produto existe
            if (produto is null)
                return Result.Failure<Guid>(new Error(
                    "Produto.NaoEncontrado",
                    $"Produto '{item.ProdutoId}' não encontrado."));

            // Valida estoque ANTES de criar — early return para evitar pedidos inválidos
            if (produto.Estoque < item.Quantidade)
                return Result.Failure<Guid>(new Error(
                    "Produto.EstoqueInsuficiente",
                    $"Estoque insuficiente para '{produto.Nome}'."));

            // Adiciona o item com preço capturado em tempo real (do serviço de Produtos)
            pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, item.Quantidade);
        }

        // 3. Confirma o pedido — muda Status para Confirmado (via regra de domínio)
        pedido.Confirmar();

        // 4. Persiste no banco de Pedidos
        await pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Publica Integration Event para RabbitMQ
        // O debito real do estoque acontece de forma ASSÍNCRONA no serviço de Produtos
        var integrationEvent = new PedidoConfirmadoIntegrationEvent(
            EventId: Guid.NewGuid(),
            OcorridoEm: DateTime.UtcNow,
            PedidoId: pedido.Id,
            ClienteId: pedido.ClienteId,
            Itens: pedido.Itens
                .Select(i => new ItemPedidoConfirmado(i.ProdutoId, i.Quantidade))
                .ToList()
                .AsReadOnly());

        await eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return Result.Success(pedido.Id);
    }
}
```

**Por que consultar o preço em Produtos ao criar o pedido?**
O preço do produto pode mudar. Ao criar o pedido, capturamos o preço atual e o armazenamos em `ItemPedido.PrecoUnitario`. O histórico do pedido fica imutável, independente de alterações futuras no catálogo.

### 8.2 IProdutosServiceClient — HTTP síncrono entre serviços

```csharp
// Abstractions/IProdutosServiceClient.cs
// Definida em Application — Application não conhece HttpClient
public interface IProdutosServiceClient
{
    Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default);
}

public sealed record ProdutoDto(Guid Id, string Nome, decimal Preco, int Estoque);
// DTO local — não depende de Produtos.Domain
```

```csharp
// Infrastructure/Http/ProdutosServiceClient.cs

// HttpClient injetado via AddHttpClient (IHttpClientFactory) — gerencia pooling de conexões
internal sealed class ProdutosServiceClient(HttpClient httpClient) : IProdutosServiceClient
{
    public async Task<ProdutoDto?> ObterProdutoAsync(
        Guid produtoId, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ProdutoDto>(
            $"api/produtos/{produtoId}", cancellationToken);
    // GetFromJsonAsync = GET + deserialização JSON + retorna null em 404
}
```

```csharp
// No InfrastructureServiceExtensions de Pedidos:
services.AddHttpClient<IProdutosServiceClient, ProdutosServiceClient>(client =>
    client.BaseAddress = new Uri(
        configuration["Services:ProdutosUrl"]
        ?? throw new InvalidOperationException("Services:ProdutosUrl não configurado.")));
// BaseAddress em dev: http://localhost:5001
// BaseAddress em Docker: http://produtos-api:8080
```

**`AddHttpClient<TInterface, TImplementation>`** — padrão typed client:
- Cria uma instância de `HttpClient` pré-configurada
- Gerencia o pool de `HttpClientHandler` internamente (evita socket exhaustion)
- O `ProdutosServiceClient` recebe o `HttpClient` pronto via injeção de dependência

### 8.3 IEventPublisher e MassTransitEventPublisher

```csharp
// Abstractions/IEventPublisher.cs

// Abstração em Application — não referencia MassTransit diretamente
// Permite trocar o broker (RabbitMQ ? Azure Service Bus) sem tocar em Application
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent;
}
```

```csharp
// Infrastructure/Messaging/MassTransitEventPublisher.cs

internal sealed class MassTransitEventPublisher(
    IPublishEndpoint publishEndpoint)  // IPublishEndpoint é do MassTransit
    : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, ct);
    // MassTransit serializa para JSON e publica no RabbitMQ exchange correto
}
```

### 8.4 PedidoConfirmadoConsumer — Recebendo o evento em Produtos

```csharp
// Infrastructure/Messaging/PedidoConfirmadoConsumer.cs (no serviço de PRODUTOS)

internal sealed class PedidoConfirmadoConsumer(
    IProdutoRepository produtoRepository,
    IUnitOfWork unitOfWork,
    ILogger<PedidoConfirmadoConsumer> logger)
    : IConsumer<PedidoConfirmadoIntegrationEvent>  // IConsumer<T> do MassTransit
{
    public async Task Consume(ConsumeContext<PedidoConfirmadoIntegrationEvent> context)
    {
        var evento = context.Message;
        logger.LogInformation("Processando PedidoConfirmado {PedidoId}...", evento.PedidoId);

        foreach (var item in evento.Itens)
        {
            var produto = await produtoRepository.ObterPorIdAsync(
                item.ProdutoId, context.CancellationToken);

            if (produto is null)
            {
                // Produto não encontrado: log de aviso + continua os outros itens
                // Não lança exceção — mensagem não deve ser reenfileirada por produto inexistente
                logger.LogWarning("Produto {ProdutoId} não encontrado.", item.ProdutoId);
                continue;
            }

            // Delega o débito ao próprio objeto de domínio (Tell, Don't Ask)
            produto.DebitarEstoque(item.Quantidade);
            // DebitarEstoque lança ProdutoException se estoque insuficiente
            // MassTransit captura a exceção e reenfileira a mensagem automaticamente
        }

        // Salva todos os débitos em uma única transação
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Estoque atualizado para o pedido {PedidoId}.", evento.PedidoId);
    }
}
```

**Garantias do MassTransit / RabbitMQ:**
- **At-least-once delivery**: se o consumer falhar sem confirmar, a mensagem é reenfileirada
- **Idempotência**: o guard em `DebitarEstoque` evita debitar mais do que o disponível em caso de reentrega
- **Dead Letter Queue**: após N tentativas com falha, a mensagem vai para uma fila de mensagens mortas para análise

---

## 9. Fluxo Completo — Do HTTP ao Banco de Dados

### 9.1 Criar Produto (CRUD simples)

```
Cliente (React/Scalar)
  ?  POST /api/produtos/
  ?  Body: { "nome": "Camiseta", "descricao": "...", "preco": 99.90, "estoque": 10 }
  ?
ExceptionHandlingMiddleware
  ?  Inicia o bloco try — qualquer exceção será capturada
  ?
ProdutosEndpoints.CriarAsync
  ?  ASP.NET Core desserializa o body JSON para CriarProdutoCommand
  ?  sender.Send(command, ct)
  ?
MediatR Pipeline
  ?
  ?? LoggingBehavior
  ?    LogInformation("Processando CriarProdutoCommand")
  ?
  ?? ValidationBehavior
  ?    CriarProdutoCommandValidator.Validate(command)
  ?    ??? FALHA ? throw ValidationException ? Middleware ? 422 + JSON de erros
  ?    ??? OK ? next(ct)
  ?
  ?? CriarProdutoCommandHandler.Handle(command, ct)
       ?
       ?? Produto.Criar(nome, desc, preco, estoque)
       ?    Guard clauses de domínio
       ?    ??? FALHA ? throw ProdutoException ? Middleware ? 400 + mensagem
       ?    ??? OK ? new Produto(Guid.NewGuid(), ...)
       ?
       ?? repository.AdicionarAsync(produto, ct)
       ?    context.Produtos.AddAsync(produto)  ? change tracker: INSERT pendente
       ?
       ?? unitOfWork.SaveChangesAsync(ct)
       ?    context.SaveChangesAsync()
       ?    ? EF Core gera e executa:
       ?    INSERT INTO Produtos (Id, Nome, Descricao, Preco, Estoque, CriadoEm)
       ?    VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
       ?
       ?? return Result.Success(produto.Id)
            ?
            ?
         MediatR retorna Result<Guid> ao Endpoint
            ?
            ?
         result.IsSuccess == true
            ?
            ?
         Results.CreatedAtRoute("ObterProduto", { id }, produto.Id)
            ?
            ?
HTTP Response:
  Status: 201 Created
  Location: /api/produtos/3fa85f64-5717-4562-b3fc-2c963f66afa6
  Body: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### 9.2 Criar Pedido (comunicação entre serviços)

```
Cliente
  ?  POST /api/pedidos/
  ?  Body: { "clienteId": "...", "itens": [{ "produtoId": "...", "quantidade": 2 }] }
  ?
CriarPedidoCommandHandler
  ?
  ?? Pedido.Criar(clienteId)   ? Pedido em memória (Status: Pendente)
  ?
  ?? Para cada item:
  ?   ?? ProdutosServiceClient.ObterProdutoAsync(produtoId)
  ?   ?    GET http://produtos-api:8080/api/produtos/{id}   ? HTTP síncrono
  ?   ?    ? { id, nome, preco: 99.90, estoque: 10 }
  ?   ?
  ?   ?? Validações: produto existe? estoque suficiente?
  ?   ?    ??? FALHA ? return Result.Failure (sem persistir nada)
  ?   ?
  ?   ?? pedido.AdicionarItem(produto.Id, "Camiseta", 99.90, 2)
  ?
  ?? pedido.Confirmar()   ? Status: Confirmado
  ?
  ?? pedidoRepository.AdicionarAsync(pedido, ct)
  ?? unitOfWork.SaveChangesAsync(ct)
  ?    INSERT INTO Pedidos + INSERT INTO Itens (owned entities)
  ?
  ?? eventPublisher.PublishAsync(PedidoConfirmadoIntegrationEvent)
       ?
       ?
  MassTransitEventPublisher.Publish(evento)
       ?  Serializa para JSON
       ?
  RabbitMQ Exchange
       ?  Roteia para a fila do consumer
       ?
       ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?
       |   SERVIÇO DE PRODUTOS (processamento assíncrono)  |
       |                                                    |
       |   PedidoConfirmadoConsumer.Consume(context)       |
       |     Para cada item do evento:                      |
       |       produto.DebitarEstoque(quantidade)           |
       |     unitOfWork.SaveChangesAsync()                  |
       |     UPDATE Produtos SET Estoque = Estoque - 2      |
       |     WHERE Id = @produtoId                          |
       ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?

HTTP Response para o cliente:
  Status: 201 Created
  Body: "{pedidoId}"
  ? retorna ANTES do consumer terminar (comunicação eventual)
```

**Consistência eventual:** O cliente recebe `201 Created` assim que o pedido é salvo e o evento publicado. O débito do estoque acontece segundos depois, de forma assíncrona. Esse é o trade-off da arquitetura orientada a eventos: maior disponibilidade em troca de consistência imediata.

---

## 10. Infraestrutura Docker

```yaml
# docker-compose.yml (resumo dos principais serviços)

services:
  sqlserver-produtos:          # banco isolado para Produtos
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
    volumes:
      - sqlserver-produtos-data:/var/opt/mssql  # persiste dados entre reinicializações

  sqlserver-pedidos:           # banco isolado para Pedidos
    image: mcr.microsoft.com/mssql/server:2022-latest
    # configuração análoga ao de produtos

  rabbitmq:                    # broker de mensagens
    image: rabbitmq:3-management
    ports:
      - "5672:5672"   # porta AMQP (protocolo de mensagens)
      - "15672:15672" # porta da interface de administração web

  produtos-api:
    build: ./services/Produtos/Produtos.Api
    environment:
      ConnectionStrings__ProdutosConnection: "Server=sqlserver-produtos;..."
      RabbitMQ__Host: rabbitmq
    depends_on:
      sqlserver-produtos: { condition: service_healthy }
      rabbitmq:           { condition: service_healthy }

  pedidos-api:
    build: ./services/Pedidos/Pedidos.Api
    environment:
      Services__ProdutosUrl: "http://produtos-api:8080"
      # Pedidos chama Produtos pelo nome do serviço Docker (DNS interno)
    depends_on:
      sqlserver-pedidos: { condition: service_healthy }
      rabbitmq:          { condition: service_healthy }
```

**`depends_on` com `condition: service_healthy`:** garante que os containers dependentes só iniciam quando os serviços de banco e RabbitMQ estiverem respondendo ao healthcheck, evitando falhas de conexão no startup.

**Resolução de nome entre containers:** dentro da rede Docker, `produtos-api` resolve para o IP do container de mesmo nome. O Pedidos não precisa conhecer o IP — o Docker DNS resolve automaticamente.

---

## 11. Diagrama de Dependências entre Projetos

```
Arquitetura.SharedKernel
  ? referenciado por todos os projetos Domain e Application
  ?
  ??? Produtos.Domain
  ?     ?
  ?     ??? Produtos.Application
  ?     ?     ?
  ?     ?     ??? Produtos.Infrastructure  (implementa abstrações de Application)
  ?     ?     ??? Produtos.Api             (consome Application via MediatR)
  ?     ?
  ?     ??? (não referencia Infrastructure nem Api)
  ?
  ??? Pedidos.Domain
        ?
        ??? Pedidos.Application
        ?     ?
        ?     ??? Pedidos.Infrastructure   (implementa abstrações de Application)
        ?     ??? Pedidos.Api              (consome Application via MediatR)
        ?
        ??? (não referencia Infrastructure nem Api)

Comunicação entre serviços:
  Pedidos.Infrastructure.Http  ?  (HTTP)     ?  Produtos.Api
  Pedidos.Infrastructure.Msg   ?  (RabbitMQ) ?  Produtos.Infrastructure.Messaging
```

**Regra de dependência (Clean Architecture):** as setas apontam sempre para dentro. Nenhuma camada interna conhece camadas externas. O Domain é completamente isolado. A Infrastructure conhece o Domain e o Application, mas nunca é conhecida por eles — apenas implementa as interfaces definidas em Application.

---

*Documentação gerada para o backend do projeto. Para a documentação do frontend, consulte `arquitetura.client/FRONTEND.md`. Para a visão geral completa da solução, consulte `README.md` na raiz.*
