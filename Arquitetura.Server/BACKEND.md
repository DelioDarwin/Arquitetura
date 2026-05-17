# Documentação do Backend — Arquitetura de Microserviços .NET 10

> Guia de estudo detalhado que explica cada camada do backend, do recebimento de uma requisição HTTP até a persistência no banco de dados e a troca de mensagens entre serviços. O CRUD de **Produtos** é usado como caso de estudo principal, com o fluxo de **Pedidos** explicando a comunicação entre serviços, incluindo a integração com a API pública **ViaCEP**.

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
9. [Consulta de CEP — Integração com ViaCEP](#9-consulta-de-cep--integração-com-viacep)
   - 9.1 [IViaCepClient — Abstração em Application](#91-iviacepclient--abstração-em-application)
   - 9.2 [ConsultarCepQuery — Handler CQRS](#92-consultarcepquery--handler-cqrs)
   - 9.3 [ViaCepClient — Implementação HTTP em Infrastructure](#93-viacepcliente--implementação-http-em-infrastructure)
   - 9.4 [CepEndpoints — Rota exposta na Pedidos API](#94-cependpoints--rota-exposta-na-pedidos-api)
   - 9.5 [Registro no InfrastructureServiceExtensions](#95-registro-no-infrastructureserviceextensions)
10. [Fluxo Completo — Do HTTP ao Banco de Dados](#10-fluxo-completo--do-http-ao-banco-de-dados)
    - 10.1 [Criar Produto (CRUD simples)](#101-criar-produto-crud-simples)
    - 10.2 [Criar Pedido (comunicação entre serviços)](#102-criar-pedido-comunicação-entre-serviços)
    - 10.3 [Consultar CEP (integração com API externa)](#103-consultar-cep-integração-com-api-externa)
11. [Infraestrutura Docker](#11-infraestrutura-docker)
12. [Diagrama de Dependências entre Projetos](#12-diagrama-de-dependências-entre-projetos)

---

## 1. Visão Geral da Arquitetura Backend

O backend é composto por dois microserviços independentes que seguem os mesmos princípios arquiteturais. Cada serviço tem quatro camadas internas organizadas em **Clean Architecture**:

```
+-------------------------------------------------------------+
¦  API  (Produtos.Api / Pedidos.Api)                          ¦
¦  Minimal APIs, Endpoints, Middleware, Program.cs            ¦
¦  ? Recebe requisições HTTP, delega ao MediatR               ¦
+-------------------------------------------------------------¦
¦  APPLICATION  (Produtos.Application / Pedidos.Application)  ¦
¦  Commands, Queries, Handlers, Validators, Behaviors         ¦
¦  ? Orquestra casos de uso, sem detalhes de infra            ¦
+-------------------------------------------------------------¦
¦  INFRASTRUCTURE  (Produtos.Infra / Pedidos.Infra)           ¦
¦  DbContext, Repositories, Migrations, RabbitMQ, HTTP        ¦
¦  ? Implementações concretas das abstrações                  ¦
+-------------------------------------------------------------¦
¦  DOMAIN  (Produtos.Domain / Pedidos.Domain)                 ¦
¦  Entidades, Enums, Exceções de domínio                      ¦
¦  ? Regras de negócio puras, zero dependências externas      ¦
+-------------------------------------------------------------¦
¦           ? todas as camadas dependem desta ?               ¦
+-------------------------------------------------------------¦
¦  SHAREDKERNEL  (Arquitetura.SharedKernel)                   ¦
¦  Entity, ValueObject, Result, Error, IIntegrationEvent      ¦
¦  ? Contratos e primitivos compartilhados entre serviços     ¦
+-------------------------------------------------------------+
```

**Princípio fundamental:** as dependências apontam sempre para dentro. Domain não conhece Application. Application não conhece Infrastructure. A API só conhece Application. Infrastructure implementa interfaces definidas em Application.

---

## 2. Estrutura de Projetos

```
Arquitetura/
+-- shared/
¦   +-- Arquitetura.SharedKernel/          ? primitivos e contratos compartilhados
¦       +-- Common/
¦       ¦   +-- Error.cs
¦       ¦   +-- Result.cs
¦       +-- Primitives/
¦       ¦   +-- Entity.cs
¦       ¦   +-- ValueObject.cs
¦       ¦   +-- IDomainEvent.cs
¦       +-- Messaging/
¦       ¦   +-- IIntegrationEvent.cs
¦       ¦   +-- PedidoConfirmadoIntegrationEvent.cs
¦       +-- Exceptions/
¦           +-- DomainException.cs
¦
+-- services/
¦   +-- Produtos/
¦   ¦   +-- Produtos.Domain/               ? entidade Produto, exceções
¦   ¦   +-- Produtos.Application/          ? commands, queries, behaviors
¦   ¦   +-- Produtos.Infrastructure/       ? EF Core, migrations, RabbitMQ consumer
¦   ¦   +-- Produtos.Api/                  ? endpoints, middleware, Program.cs
¦   ¦
¦   +-- Pedidos/
¦       +-- Pedidos.Domain/                ? entidades Pedido/ItemPedido, enum status
¦       +-- Pedidos.Application/           ? criar pedido, listar, consultar CEP, abstrações
¦       +-- Pedidos.Infrastructure/        ? EF Core, HTTP clients (Produtos + ViaCEP), RabbitMQ
¦       +-- Pedidos.Api/                   ? endpoints (pedidos + CEP), middleware, Program.cs
¦
+-- docker-compose.yml                     ? orquestração de containers
+-- Arquitetura.Server/
    +-- BACKEND.md                         ? este arquivo
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
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;
    protected Entity() { }  // construtor vazio necessário para o EF Core

    public Guid Id { get; private init; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

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
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other) =>
        other is not null && GetAtomicValues().SequenceEqual(other.GetAtomicValues());

    public override int GetHashCode() =>
        GetAtomicValues()
            .Aggregate(0, (hash, value) =>
                HashCode.Combine(hash, value?.GetHashCode() ?? 0));

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
    protected Result(bool isSuccess, Error error)
    {
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

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado de falha.");
}
```

**Por que usar Result Pattern em vez de lançar exceções?**

```csharp
// ? Sem Result Pattern — exceções para fluxos esperados
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
```

Exceções devem representar situações **inesperadas** (banco caiu, timeout de rede). "Produto não encontrado" é algo esperado e deve ser tratado como dado, não como exceção.

### 3.4 Error — Erros tipados e identificáveis

```csharp
// shared/Arquitetura.SharedKernel/Common/Error.cs

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
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

// IIntegrationEvent — contrato para eventos ENTRE serviços via broker
public interface IIntegrationEvent
{
    Guid EventId { get; }        // idempotência: permite detectar duplicatas
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
public abstract class DomainException(string message) : Exception(message);

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
    private Produto() { }  // construtor para o EF Core

    private Produto(Guid id, string nome, string descricao, decimal preco, int estoque)
        : base(id)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    // Factory Method — único ponto de entrada para criar um Produto válido
    public static Produto Criar(string nome, string descricao, decimal preco, int estoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ProdutoException("O nome do produto é obrigatório.");
        if (preco <= 0)
            throw new ProdutoException("O preço deve ser maior que zero.");
        if (estoque < 0)
            throw new ProdutoException("O estoque não pode ser negativo.");

        return new Produto(Guid.NewGuid(), nome.Trim(), descricao.Trim(), preco, estoque);
    }

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
        AtualizadoEm = DateTime.UtcNow;
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
- **Tell, Don't Ask**: em vez de ler `produto.Estoque` e subtrair externamente, pedimos `produto.DebitarEstoque(n)`
- **Factory Method** (`Criar`): garante que nenhum `Produto` inválido seja criado
- **Encapsulamento total**: `private set` em todas as propriedades

### 4.2 Pedido e ItemPedido — Agregado de Pedidos

```csharp
// services/Pedidos/Pedidos.Domain/Entities/Pedido.cs

public sealed class Pedido : Entity
{
    private readonly List<ItemPedido> _itens = [];

    private Pedido(Guid id, Guid clienteId) : base(id)
    {
        ClienteId = clienteId;
        Status = StatusPedido.Pendente;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    // Propriedade calculada — não persiste no banco
    public decimal Total => _itens.Sum(i => i.PrecoUnitario * i.Quantidade);

    public static Pedido Criar(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
            throw new PedidoException("O clienteId é obrigatório.");
        return new Pedido(Guid.NewGuid(), clienteId);
    }

    public void AdicionarItem(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        if (Status != StatusPedido.Pendente)
            throw new PedidoException("Só é possível adicionar itens em pedidos pendentes.");

        // Idempotência: se o produto já existe no pedido, soma a quantidade
        var itemExistente = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente is not null)
        {
            _itens.Remove(itemExistente);
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

// record = igualdade por valor + imutabilidade; owned entity no EF Core
public sealed record ItemPedido(
    Guid Id,
    Guid PedidoId,
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoUnitario,
    int Quantidade);
```

### 4.3 StatusPedido — Enum de estado

```csharp
public enum StatusPedido
{
    Pendente   = 0,  // estado inicial
    Confirmado = 1,  // trigger para debitar estoque via RabbitMQ
    Cancelado  = 2,
    Entregue   = 3
}
```

**Valores explícitos** garantem que reordenar o enum no código não corrompa dados já persistidos no banco.

---

## 5. Camada Application — Casos de Uso com CQRS

```
services/Produtos/Produtos.Application/
services/Pedidos/Pedidos.Application/
```

### 5.1 O que é CQRS

**CQRS (Command Query Responsibility Segregation)** separa operações de escrita (Commands) de operações de leitura (Queries):

```
Leitura  ? Query   ? QueryHandler   ? Retorna dados (sem efeitos colaterais)
Escrita  ? Command ? CommandHandler ? Modifica estado (sem retornar dados desnecessários)
```

O **MediatR** atua como mediador — o código que quer executar algo não conhece quem vai processar:

```csharp
var result = await sender.Send(new CriarProdutoCommand(nome, desc, preco, estoque), ct);
//                                                ?
//                              MediatR encontra CriarProdutoCommandHandler e executa
```

### 5.2 Commands — Criar Produto (caso de estudo completo)

**Passo 1: O Command**

```csharp
// record imutável que carrega a intenção de negócio
public sealed record CriarProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque) : IRequest<Result<Guid>>;
```

**Passo 2: O Validator**

```csharp
internal sealed class CriarProdutoCommandValidator : AbstractValidator<CriarProdutoCommand>
{
    public CriarProdutoCommandValidator()
    {
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

**Por que validar em Application E no Domain?**
- O **Validator** valida a _forma_ da requisição (campos obrigatórios, tamanho máximo) — antes de instanciar objetos de domínio
- O **Domain** valida as _invariantes de negócio_ — segunda linha de defesa

**Passo 3: O Handler**

```csharp
internal sealed class CriarProdutoCommandHandler(
    IProdutoRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CriarProdutoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        // 1. Delega a criação ao Domain (que aplica as invariantes)
        var produto = Produto.Criar(
            request.Nome, request.Descricao, request.Preco, request.Estoque);

        // 2. Persiste via abstração (Repository Pattern)
        await repository.AdicionarAsync(produto, cancellationToken);

        // 3. Confirma a transação (Unit of Work Pattern)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Retorna o Id encapsulado em Result
        return Result.Success(produto.Id);
    }
}
```

### 5.3 Queries — Listar e Obter Produto

```csharp
// Query sem parâmetros — lista tudo
public sealed record ListarProdutosQuery : IRequest<Result<IReadOnlyList<Produto>>>;

// Query com parâmetro — busca pelo Id
public sealed record ObterProdutoPorIdQuery(Guid Id) : IRequest<Result<Produto>>;
```

```csharp
internal sealed class ListarProdutosQueryHandler(IProdutoRepository repository)
    : IRequestHandler<ListarProdutosQuery, Result<IReadOnlyList<Produto>>>
{
    public async Task<Result<IReadOnlyList<Produto>>> Handle(
        ListarProdutosQuery request, CancellationToken cancellationToken)
    {
        var produtos = await repository.ListarAsync(cancellationToken);
        return Result.Success(produtos);
    }
}
```

### 5.4 Pipeline de Behaviors do MediatR

```
Requisição HTTP
      ?
  LoggingBehavior    ? registrado primeiro = mais externo
      ?
  ValidationBehavior ? executa antes do Handler
      ?
  CommandHandler / QueryHandler
      ?  (resposta sobe o pipeline na ordem inversa)
  ValidationBehavior
      ?
  LoggingBehavior
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
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processando {RequestName}", typeof(TRequest).Name);
        var response = await next(cancellationToken);
        logger.LogInformation("Concluído {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

**ValidationBehavior:**

```csharp
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
```

### 5.5 Abstrações — Repository e UnitOfWork

```csharp
// Definidas em Application — implementadas em Infrastructure
public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Produto produto, CancellationToken ct = default);
    void Atualizar(Produto produto);  // síncrono — EF Core já rastreia o objeto
    void Remover(Produto produto);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

**Por que `Atualizar` e `Remover` são síncronos?**
O EF Core usa um Change Tracker em memória. `Update()` e `Remove()` apenas marcam o estado — o SQL só é gerado quando `SaveChangesAsync()` é chamado.

### 5.6 Registro de Serviços — ApplicationServiceExtensions

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    var assembly = typeof(ApplicationServiceExtensions).Assembly;

    // Registra todos os Handlers, Commands e Queries do assembly
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

    // Behaviors na ordem correta: Logging primeiro, depois Validation
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Registra todos os AbstractValidator<T> do assembly automaticamente
    services.AddValidatorsFromAssembly(assembly);

    return services;
}
```

---

## 6. Camada Infrastructure — Implementação Concreta

```
services/Produtos/Produtos.Infrastructure/
services/Pedidos/Pedidos.Infrastructure/
```

### 6.1 ProdutosDbContext — O contexto do EF Core

```csharp
// Data/ProdutosDbContext.cs

// Implementa IUnitOfWork — o próprio DbContext é a unidade de trabalho
public sealed class ProdutosDbContext(DbContextOptions<ProdutosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as IEntityTypeConfiguration<T> do assembly automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

**Por que `IUnitOfWork` é implementado pelo `DbContext`?**
O `DbContext` do EF Core já é uma Unit of Work nativa. Em vez de criar uma classe `UnitOfWork` extra que apenas delega, registramos o próprio `DbContext` como `IUnitOfWork`:

```csharp
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProdutosDbContext>());
// Mesma instância scoped — garante consistência transacional
```

### 6.2 ProdutoConfiguration — Mapeamento da tabela

```csharp
internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.Preco)
            .HasPrecision(18, 2);  // DECIMAL(18,2) — crítico para valores monetários

        builder.Property(p => p.CriadoEm)
            .IsRequired();
    }
}
```

**Por que `HasPrecision(18, 2)` é crítico?**
- `FLOAT/DOUBLE` são representações binárias: `0.1 + 0.2 = 0.30000000000000004`
- `DECIMAL(18, 2)` armazena como inteiro com escala: `0.1 + 0.2 = 0.30` exato
- **Nunca use `float/double` para dinheiro**

### 6.3 ProdutoRepository — Acesso aos dados

```csharp
internal sealed class ProdutoRepository(ProdutosDbContext context) : IProdutoRepository
{
    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);

    // AsNoTracking() — ~30% mais rápido para leituras que não serão modificadas
    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default) =>
        await context.Produtos.AsNoTracking().ToListAsync(ct);

    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default) =>
        await context.Produtos.AddAsync(produto, ct);

    public void Atualizar(Produto produto) =>
        context.Produtos.Update(produto);

    public void Remover(Produto produto) =>
        context.Produtos.Remove(produto);
}
```

**`AsNoTracking()` — quando usar:**

| Operação | AsNoTracking? | Motivo |
|----------|--------------|--------|
| Listar para exibição | ? Sim | Não vai modificar — economiza memória |
| Buscar para editar | ? Não | Precisa rastrear mudanças para gerar UPDATE |
| Buscar para deletar | ? Não | Precisa estar rastreado para gerar DELETE |

### 6.4 Migrations — Versionamento do banco de dados

```csharp
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

**Comandos úteis:**
```bash
# Criar nova migration após mudar o modelo
dotnet ef migrations add NomeDaMigration --project Produtos.Infrastructure --startup-project Produtos.Api

# Aplicar migrations pendentes
dotnet ef database update --project Produtos.Infrastructure --startup-project Produtos.Api

# Gerar script SQL para revisão antes de produção
dotnet ef migrations script --project Produtos.Infrastructure --startup-project Produtos.Api
```

### 6.5 MigrationExtensions — Migrations automáticas no startup

```csharp
public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        // CreateScope() necessário — IServiceProvider raiz não resolve serviços scoped
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProdutosDbContext>();

        // Idempotente: migrations já aplicadas são ignoradas
        await db.Database.MigrateAsync();
    }
}
```

Chamado em `Program.cs` antes de `app.Run()` para garantir banco atualizado ao iniciar o container.

### 6.6 InfrastructureServiceExtensions — Registro do EF e MassTransit

O serviço de **Pedidos** registra três clientes HTTP distintos — cada um com responsabilidade própria:

```csharp
// services/Pedidos/Pedidos.Infrastructure/InfrastructureServiceExtensions.cs

public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // EF Core
    services.AddDbContext<PedidosDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("PedidosConnection")));

    services.AddScoped<IPedidoRepository, PedidoRepository>();
    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PedidosDbContext>());

    // HTTP Client — comunicação síncrona com o serviço de Produtos
    services.AddHttpClient<IProdutosServiceClient, ProdutosServiceClient>(client =>
        client.BaseAddress = new Uri(configuration["Services:ProdutosUrl"]
            ?? throw new InvalidOperationException("Services:ProdutosUrl não configurado.")));

    // HTTP Client — integração com a API pública ViaCEP
    services.AddHttpClient<IViaCepClient, ViaCepClient>(client =>
        client.BaseAddress = new Uri("https://viacep.com.br/"));

    // Publisher de Integration Events via MassTransit / RabbitMQ
    services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

    services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(configuration["RabbitMQ:Password"] ?? "guest");
            });

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
services/Pedidos/Pedidos.Api/
```

### 7.1 Program.cs — Bootstrap da aplicação

**Produtos API:**
```csharp
// services/Produtos/Produtos.Api/Program.cs

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference(opt => opt.WithTitle("Produtos API"));

app.MapProdutosEndpoints();
app.MapHealthChecks("/health");

app.Run();
```

**Pedidos API** — registra dois grupos de endpoints:
```csharp
// services/Pedidos/Pedidos.Api/Program.cs

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference(opt => opt.WithTitle("Pedidos API"));

app.MapPedidosEndpoints();   // ? /api/pedidos
app.MapCepEndpoints();        // ? /api/cep
app.MapHealthChecks("/health");

app.Run();
```

**A ordem do middleware é crítica.** O `ExceptionHandlingMiddleware` deve ser registrado antes dos endpoints para capturar exceções lançadas por qualquer rota.

### 7.2 Minimal APIs — ProdutosEndpoints

```csharp
public static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/produtos").WithTags("Produtos");

        group.MapGet("/", ListarAsync).WithSummary("Lista todos os produtos");
        group.MapGet("/{id:guid}", ObterPorIdAsync)
             .WithName("ObterProduto")
             .WithSummary("Obtém produto por Id");
        group.MapPost("/", CriarAsync).WithSummary("Cria um novo produto");
        group.MapPut("/{id:guid}", AtualizarAsync).WithSummary("Atualiza um produto");
        group.MapDelete("/{id:guid}", DeletarAsync).WithSummary("Remove um produto");

        return app;
    }

    private static async Task<IResult> CriarAsync(
        CriarProdutoCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.CreatedAtRoute("ObterProduto", new { id = result.Value }, result.Value)
            : Results.BadRequest(result.Error);
    }

    // ... demais métodos seguem o mesmo padrão
}
```

**Tabela de status HTTP:**

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
internal sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)  // lançada pelo ValidationBehavior
        {
            logger.LogWarning("Erro de validação: {Errors}", ex.Errors);
            await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity,
                "Erro de Validação",
                ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (DomainException ex)  // lançada pelas entidades de domínio
        {
            logger.LogWarning("Regra de negócio violada: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.BadRequest,
                "Regra de Negócio", [ex.Message]);
        }
        catch (Exception ex)  // qualquer exceção inesperada
        {
            logger.LogError(ex, "Erro inesperado");
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                "Erro Interno",
                ["Ocorreu um erro inesperado. Tente novamente mais tarde."]);
        }
    }
}
```

**Exemplo de resposta (RFC 7807 — Problem Details):**
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

O **Scalar** substitui o Swagger UI com uma interface mais moderna:
- Produtos API: `http://localhost:5001/scalar/v1`
- Pedidos API: `http://localhost:5002/scalar/v1`

```csharp
app.MapOpenApi();                         // GET /openapi/v1.json
app.MapScalarApiReference(opt =>
    opt.WithTitle("Pedidos API"));        // GET /scalar/v1
```

---

## 8. Serviço de Pedidos — Comunicação entre Microserviços

O serviço de Pedidos demonstra dois padrões de comunicação:
1. **Síncrono (HTTP)**: Pedidos consulta Produtos para validar preço e estoque antes de criar o pedido
2. **Assíncrono (RabbitMQ)**: Após confirmar o pedido, publica um evento que Produtos consome para debitar o estoque

### 8.1 CriarPedidoCommandHandler — O fluxo mais complexo

```csharp
// services/Pedidos/Pedidos.Application/Features/Commands/CriarPedido/CriarPedidoCommandHandler.cs

public sealed record CriarPedidoCommand(
    Guid ClienteId,
    List<ItemPedidoDto> Itens) : IRequest<Result<Guid>>;

public sealed record ItemPedidoDto(Guid ProdutoId, int Quantidade);

internal sealed class CriarPedidoCommandHandler(
    IPedidoRepository pedidoRepository,
    IProdutosServiceClient produtosClient,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : IRequestHandler<CriarPedidoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        // 1. Cria o agregado Pedido em estado Pendente
        var pedido = Pedido.Criar(request.ClienteId);

        // 2. Para cada item, consulta o serviço de Produtos via HTTP
        foreach (var item in request.Itens)
        {
            var produto = await produtosClient.ObterProdutoAsync(item.ProdutoId, cancellationToken);

            if (produto is null)
                return Result.Failure<Guid>(new Error(
                    "Produto.NaoEncontrado",
                    $"Produto '{item.ProdutoId}' não encontrado."));

            if (produto.Estoque < item.Quantidade)
                return Result.Failure<Guid>(new Error(
                    "Produto.EstoqueInsuficiente",
                    $"Estoque insuficiente para '{produto.Nome}'."));

            // Preço capturado em tempo real — fica imutável no ItemPedido
            pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, item.Quantidade);
        }

        // 3. Confirma o pedido — Status: Pendente ? Confirmado
        pedido.Confirmar();

        // 4. Persiste no banco de Pedidos
        await pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Publica Integration Event — débito do estoque é assíncrono
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

### 8.2 IProdutosServiceClient — HTTP síncrono entre serviços

```csharp
// Application — abstração desacoplada de HttpClient
public interface IProdutosServiceClient
{
    Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default);
}

public sealed record ProdutoDto(Guid Id, string Nome, decimal Preco, int Estoque);
```

```csharp
// Infrastructure/Http/ProdutosServiceClient.cs
internal sealed class ProdutosServiceClient(HttpClient httpClient) : IProdutosServiceClient
{
    public async Task<ProdutoDto?> ObterProdutoAsync(
        Guid produtoId, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ProdutoDto>(
            $"api/produtos/{produtoId}", cancellationToken);
    // GetFromJsonAsync retorna null em 404 automaticamente
}
```

```csharp
// Registro com BaseAddress configurável por ambiente
services.AddHttpClient<IProdutosServiceClient, ProdutosServiceClient>(client =>
    client.BaseAddress = new Uri(
        configuration["Services:ProdutosUrl"]
        ?? throw new InvalidOperationException("Services:ProdutosUrl não configurado.")));
// Dev:    http://localhost:5001
// Docker: http://produtos-api:8080
```

### 8.3 IEventPublisher e MassTransitEventPublisher

```csharp
// Abstração em Application — não referencia MassTransit diretamente
// Permite trocar o broker sem tocar em Application
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent;
}
```

```csharp
internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, ct);
}
```

### 8.4 PedidoConfirmadoConsumer — Recebendo o evento em Produtos

```csharp
// Infrastructure/Messaging/PedidoConfirmadoConsumer.cs (no serviço de PRODUTOS)

internal sealed class PedidoConfirmadoConsumer(
    IProdutoRepository produtoRepository,
    IUnitOfWork unitOfWork,
    ILogger<PedidoConfirmadoConsumer> logger)
    : IConsumer<PedidoConfirmadoIntegrationEvent>
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
                logger.LogWarning("Produto {ProdutoId} não encontrado.", item.ProdutoId);
                continue;  // não reprocessa — produto inexistente não deve bloquear a fila
            }

            // Tell, Don't Ask — a entidade valida e executa
            produto.DebitarEstoque(item.Quantidade);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Estoque atualizado para o pedido {PedidoId}.", evento.PedidoId);
    }
}
```

**Garantias do MassTransit / RabbitMQ:**
- **At-least-once delivery**: se o consumer falhar sem confirmar, a mensagem é reenfileirada
- **Dead Letter Queue**: após N tentativas com falha, a mensagem vai para análise manual

---

## 9. Consulta de CEP — Integração com ViaCEP

O serviço de Pedidos integra com a **API pública ViaCEP** (`https://viacep.com.br`) para consulta de endereços por CEP. Essa funcionalidade é útil para preenchimento automático do endereço de entrega ao criar um pedido.

A integração segue exatamente os mesmos princípios arquiteturais do restante do sistema: abstração em Application, implementação em Infrastructure, endpoint em Api.

### 9.1 IViaCepClient — Abstração em Application

```csharp
// services/Pedidos/Pedidos.Application/Abstractions/IViaCepClient.cs

/// <summary>
/// Contrato para consulta de endereço por CEP via API externa (ViaCEP).
/// Desacopla Pedidos.Application da implementação HTTP concreta.
/// </summary>
public interface IViaCepClient
{
    Task<ViaCepDto?> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO que representa o retorno da API ViaCEP.
/// </summary>
public sealed record ViaCepDto(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    string Ibge,
    string Gia,
    string Ddd,
    string Siafi);
```

O `ViaCepDto` é definido em Application — a camada de domínio não conhece detalhes de APIs externas. O record garante imutabilidade dos dados retornados.

### 9.2 ConsultarCepQuery — Handler CQRS

```csharp
// services/Pedidos/Pedidos.Application/Features/Queries/ConsultarCep/ConsultarCepQuery.cs

/// <summary>
/// Query para consultar o endereço de um CEP via API externa ViaCEP.
/// </summary>
public sealed record ConsultarCepQuery(string Cep) : IRequest<Result<ViaCepDto>>;

internal sealed class ConsultarCepQueryHandler(IViaCepClient viaCepClient)
    : IRequestHandler<ConsultarCepQuery, Result<ViaCepDto>>
{
    public async Task<Result<ViaCepDto>> Handle(
        ConsultarCepQuery request, CancellationToken cancellationToken)
    {
        var endereco = await viaCepClient.ConsultarCepAsync(request.Cep, cancellationToken);

        if (endereco is null)
            return Result.Failure<ViaCepDto>(
                new Error("Cep.NaoEncontrado", $"CEP '{request.Cep}' não encontrado."));

        return Result.Success(endereco);
    }
}
```

**Por que é uma Query e não um Command?**
Consultar um CEP é uma operação de leitura pura — não modifica nenhum estado. Segue o princípio do CQRS: Queries só lêem, Commands só escrevem.

### 9.3 ViaCepClient — Implementação HTTP em Infrastructure

```csharp
// services/Pedidos/Pedidos.Infrastructure/Http/ViaCepClient.cs

internal sealed class ViaCepClient(HttpClient httpClient) : IViaCepClient
{
    public async Task<ViaCepDto?> ConsultarCepAsync(
        string cep, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"ws/{cep}/json/", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken);

        // ViaCEP retorna { "erro": true } para CEPs inválidos mesmo com HTTP 200
        if (result is null || result.Erro == true)
            return null;

        return new ViaCepDto(
            result.Cep ?? string.Empty,
            result.Logradouro ?? string.Empty,
            result.Complemento ?? string.Empty,
            result.Bairro ?? string.Empty,
            result.Localidade ?? string.Empty,
            result.Uf ?? string.Empty,
            result.Ibge ?? string.Empty,
            result.Gia ?? string.Empty,
            result.Ddd ?? string.Empty,
            result.Siafi ?? string.Empty);
    }

    // Modelo interno de deserialização — não exposto fora desta classe
    private sealed class ViaCepResponse
    {
        [JsonPropertyName("cep")]        public string? Cep { get; init; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; init; }
        [JsonPropertyName("complemento")]public string? Complemento { get; init; }
        [JsonPropertyName("bairro")]     public string? Bairro { get; init; }
        [JsonPropertyName("localidade")] public string? Localidade { get; init; }
        [JsonPropertyName("uf")]         public string? Uf { get; init; }
        [JsonPropertyName("ibge")]       public string? Ibge { get; init; }
        [JsonPropertyName("gia")]        public string? Gia { get; init; }
        [JsonPropertyName("ddd")]        public string? Ddd { get; init; }
        [JsonPropertyName("siafi")]      public string? Siafi { get; init; }
        [JsonPropertyName("erro")]       public bool? Erro { get; init; }
    }
}
```

**Detalhe importante — `{ "erro": true }`:**
A API ViaCEP retorna HTTP 200 mesmo para CEPs inexistentes, incluindo `"erro": true` no body. O `ViaCepClient` trata esse caso explicitamente, convertendo-o em `null` para que o handler aplique o `Result.Failure` correto.

**Por que `GetAsync` em vez de `GetFromJsonAsync`?**
`GetFromJsonAsync` lançaria exceção em respostas não-2xx. Aqui usamos `GetAsync` para checar o status code manualmente antes de tentar deserializar, tornando o tratamento de erro explícito e controlado.

### 9.4 CepEndpoints — Rota exposta na Pedidos API

```csharp
// services/Pedidos/Pedidos.Api/Endpoints/CepEndpoints.cs

public static class CepEndpoints
{
    public static IEndpointRouteBuilder MapCepEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cep").WithTags("CEP");

        group.MapGet("/{cep}", ConsultarAsync)
             .WithName("ConsultarCep")
             .WithSummary("Consulta o endereço de um CEP")
             .WithDescription("Retorna os dados de endereço usando a API pública ViaCEP (https://viacep.com.br).");

        return app;
    }

    private static async Task<IResult> ConsultarAsync(string cep, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ConsultarCepQuery(cep), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }
}
```

**Exemplo de uso:**
```
GET /api/cep/01310-100

HTTP 200 OK
{
    "cep": "01310-100",
    "logradouro": "Avenida Paulista",
    "complemento": "de 1 a 610 - lado par",
    "bairro": "Bela Vista",
    "localidade": "São Paulo",
    "uf": "SP",
    "ibge": "3550308",
    "gia": "1004",
    "ddd": "11",
    "siafi": "7107"
}
```

```
GET /api/cep/00000-000

HTTP 404 Not Found
{
    "code": "Cep.NaoEncontrado",
    "description": "CEP '00000-000' não encontrado."
}
```

### 9.5 Registro no InfrastructureServiceExtensions

```csharp
// BaseAddress fixo — ViaCEP é uma API pública sem configuração de ambiente
services.AddHttpClient<IViaCepClient, ViaCepClient>(client =>
    client.BaseAddress = new Uri("https://viacep.com.br/"));
```

**Por que `AddHttpClient` e não `new HttpClient()`?**
`AddHttpClient` usa `IHttpClientFactory` internamente, que gerencia o pool de `HttpMessageHandler`. Isso evita o problema de **socket exhaustion** (esgotamento de portas TCP) que ocorre ao criar `HttpClient` diretamente em loops ou requisições frequentes.

---

## 10. Fluxo Completo — Do HTTP ao Banco de Dados

### 10.1 Criar Produto (CRUD simples)

```
Cliente (React/Scalar)
  ?  POST /api/produtos/
  ?  Body: { "nome": "Camiseta", "preco": 99.90, "estoque": 10 }
  ?
ExceptionHandlingMiddleware
  ?  inicia bloco try
  ?
ProdutosEndpoints.CriarAsync
  ?  sender.Send(CriarProdutoCommand, ct)
  ?
MediatR Pipeline
  ?
  +- LoggingBehavior ? LogInformation("Processando CriarProdutoCommand")
  ?
  +- ValidationBehavior ? CriarProdutoCommandValidator.Validate(command)
  ¦     +- FALHA ? throw ValidationException ? Middleware ? 422 + erros
  ¦     +- OK ? next(ct)
  ?
  +- CriarProdutoCommandHandler.Handle(command, ct)
       ?
       +- Produto.Criar(nome, desc, preco, estoque)
       ¦     +- FALHA ? throw ProdutoException ? Middleware ? 400
       ¦     +- OK ? new Produto(Guid.NewGuid(), ...)
       ?
       +- repository.AdicionarAsync(produto)   ? change tracker: INSERT pendente
       ?
       +- unitOfWork.SaveChangesAsync()
              ? EF Core gera:
            INSERT INTO Produtos (Id, Nome, Preco, Estoque, CriadoEm) VALUES (...)
              ?
           return Result.Success(produto.Id)
              ?
           Results.CreatedAtRoute("ObterProduto", { id }, guid)

HTTP Response: 201 Created | Location: /api/produtos/{id} | Body: "{guid}"
```

### 10.2 Criar Pedido (comunicação entre serviços)

```
Cliente
  ?  POST /api/pedidos/
  ?  Body: { "clienteId": "...", "itens": [{ "produtoId": "...", "quantidade": 2 }] }
  ?
CriarPedidoCommandHandler
  ?
  +- Pedido.Criar(clienteId)  ? Pedido em memória (Status: Pendente)
  ?
  +- Para cada item:
  ¦   +- ProdutosServiceClient.ObterProdutoAsync(produtoId)
  ¦   ¦    GET http://produtos-api:8080/api/produtos/{id}  [HTTP síncrono]
  ¦   ¦    ? { id, nome, preco: 99.90, estoque: 10 }
  ¦   ¦
  ¦   +- produto existe? estoque suficiente?
  ¦   ¦    +- FALHA ? return Result.Failure (sem persistir)
  ¦   ¦
  ¦   +- pedido.AdicionarItem("Camiseta", 99.90, 2)
  ?
  +- pedido.Confirmar()  ? Status: Confirmado
  ?
  +- pedidoRepository.AdicionarAsync + unitOfWork.SaveChangesAsync()
  ¦    INSERT INTO Pedidos + INSERT INTO ItensPedido
  ?
  +- eventPublisher.PublishAsync(PedidoConfirmadoIntegrationEvent)
         ? MassTransit serializa para JSON
         ?
       RabbitMQ Exchange
         ? roteia para a fila do consumer
         ?
       +---------------------------------------------------------+
       ¦  SERVIÇO DE PRODUTOS (processamento assíncrono)         ¦
       ¦                                                         ¦
       ¦  PedidoConfirmadoConsumer.Consume(context)              ¦
       ¦    produto.DebitarEstoque(quantidade)                   ¦
       ¦    unitOfWork.SaveChangesAsync()                        ¦
       ¦    UPDATE Produtos SET Estoque = Estoque - 2 WHERE Id = ¦
       +---------------------------------------------------------+

HTTP Response: 201 Created | Body: "{pedidoId}"
? retorna ANTES do consumer terminar (consistência eventual)
```

### 10.3 Consultar CEP (integração com API externa)

```
Cliente
  ?  GET /api/cep/01310-100
  ?
ExceptionHandlingMiddleware
  ?  inicia bloco try
  ?
CepEndpoints.ConsultarAsync
  ?  sender.Send(ConsultarCepQuery("01310-100"), ct)
  ?
MediatR Pipeline
  ?
  +- LoggingBehavior ? LogInformation("Processando ConsultarCepQuery")
  ?
  +- ConsultarCepQueryHandler.Handle(query, ct)
       ?
       +- viaCepClient.ConsultarCepAsync("01310-100")
              ?
            GET https://viacep.com.br/ws/01310-100/json/
              ?
            HTTP 200 ? deserializa ViaCepResponse
              +- result.Erro == true ? return null ? Result.Failure (404)
              +- OK ? mapeia para ViaCepDto ? Result.Success
              ?
           return result.IsSuccess
              +- true  ? Results.Ok(viaCepDto)    ? 200 + JSON do endereço
              +- false ? Results.NotFound(error)  ? 404 + código de erro

HTTP Response: 200 OK | Body: { "cep": "01310-100", "logradouro": "Av. Paulista", ... }
```

---

## 11. Infraestrutura Docker

```yaml
# docker-compose.yml (resumo dos principais serviços)

services:
  sqlserver-produtos:          # banco isolado para Produtos
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
    volumes:
      - sqlserver-produtos-data:/var/opt/mssql

  sqlserver-pedidos:           # banco isolado para Pedidos
    image: mcr.microsoft.com/mssql/server:2022-latest

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"    # AMQP
      - "15672:15672"  # UI de administração

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
      ConnectionStrings__PedidosConnection: "Server=sqlserver-pedidos;..."
      Services__ProdutosUrl: "http://produtos-api:8080"
      RabbitMQ__Host: rabbitmq
    depends_on:
      sqlserver-pedidos: { condition: service_healthy }
      rabbitmq:          { condition: service_healthy }
      # Nota: ViaCEP é uma API pública — sem dependência de container
```

**`depends_on` com `condition: service_healthy`:** garante que os containers dependentes só iniciam quando banco e RabbitMQ estão respondendo ao healthcheck, evitando falhas de conexão no startup.

**Resolução de nome entre containers:** dentro da rede Docker, `produtos-api` resolve para o IP do container de mesmo nome. `pedidos-api` chama `http://produtos-api:8080` sem conhecer o IP real.

**ViaCEP e Docker:** a integração com ViaCEP usa a internet pública — não requer container adicional. O `pedidos-api` precisa apenas de acesso à rede externa, o que é o comportamento padrão do Docker.

---

## 12. Diagrama de Dependências entre Projetos

```
Arquitetura.SharedKernel
  ? referenciado por todos os projetos Domain e Application
  ¦
  +-- Produtos.Domain
  ¦     ?
  ¦     +-- Produtos.Application
  ¦     ¦     ?
  ¦     ¦     +-- Produtos.Infrastructure  (implementa IProdutoRepository, IUnitOfWork)
  ¦     ¦     +-- Produtos.Api             (consome via MediatR + ISender)
  ¦     ¦
  ¦     +-- (não referencia Infrastructure nem Api)
  ¦
  +-- Pedidos.Domain
        ?
        +-- Pedidos.Application
        ¦     ? define: IPedidoRepository, IUnitOfWork,
        ¦     ¦         IProdutosServiceClient, IViaCepClient, IEventPublisher
        ¦     ¦
        ¦     +-- Pedidos.Infrastructure  (implementa todas as abstrações acima)
        ¦     +-- Pedidos.Api             (consome via MediatR + ISender)
        ¦
        +-- (não referencia Infrastructure nem Api)

Comunicação entre serviços:
  Pedidos.Infrastructure.Http.ProdutosServiceClient  ?  (HTTP)      ?  Produtos.Api
  Pedidos.Infrastructure.Http.ViaCepClient           ?  (HTTPS)     ?  viacep.com.br (externo)
  Pedidos.Infrastructure.Messaging                   ?  (RabbitMQ)  ?  Produtos.Infrastructure.Messaging
```

**Regra de dependência (Clean Architecture):** as setas apontam sempre para dentro. O Domain é completamente isolado. A Infrastructure conhece o Domain e o Application, mas nunca é conhecida por eles — apenas implementa as interfaces definidas em Application.

---

*Documentação gerada para o backend do projeto. Para a documentação do frontend, consulte `arquitetura.client/FRONTEND.md`. Para a visão geral completa da solução, consulte `README.md` na raiz.*
