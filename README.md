# Arquitetura de Referência — Microserviços com .NET 10 e React 19

> **Template acadêmico** de uma solução completa, moderna e escalável baseada em microserviços com backend .NET 10 e frontend React 19. Cobre desde os fundamentos arquiteturais até o deploy em containers Docker com mensageria assíncrona via RabbitMQ.

---

## Sumário

1. [Visão Geral da Solução](#1-visão-geral-da-solução)
2. [Estrutura de Pastas](#2-estrutura-de-pastas)
3. [Arquiteturas e Padrões Utilizados](#3-arquiteturas-e-padrões-utilizados)
   - 3.1 [Microserviços](#31-microserviços)
   - 3.2 [Clean Architecture](#32-clean-architecture)
   - 3.3 [CQRS com MediatR](#33-cqrs-com-mediatr)
   - 3.4 [Domain-Driven Design (DDD)](#34-domain-driven-design-ddd)
   - 3.5 [Result Pattern](#35-result-pattern)
   - 3.6 [Domain Events vs Integration Events](#36-domain-events-vs-integration-events)
   - 3.7 [Database per Service](#37-database-per-service)
   - 3.8 [Shared Kernel](#38-shared-kernel)
4. [Backend — Tecnologias e Componentes](#4-backend--tecnologias-e-componentes)
   - 4.1 [.NET 10 e ASP.NET Core Minimal APIs](#41-net-10-e-aspnet-core-minimal-apis)
   - 4.2 [Entity Framework Core 10](#42-entity-framework-core-10)
   - 4.3 [MediatR 14](#43-mediatr-14)
   - 4.4 [FluentValidation 12](#44-fluentvalidation-12)
   - 4.5 [MassTransit 8 com RabbitMQ](#45-masstransit-8-com-rabbitmq)
   - 4.6 [Scalar — Interface de API no Browser](#46-scalar--interface-de-api-no-browser)
   - 4.7 [HttpClient Tipado — Comunicação entre Serviços](#47-httpclient-tipado--comunicação-entre-serviços)
   - 4.8 [Health Checks](#48-health-checks)
   - 4.9 [Middleware de Tratamento de Exceções](#49-middleware-de-tratamento-de-exceções)
   - 4.10 [ViaCEP — Integração com API Pública de CEP](#410-viacep--integração-com-api-pública-de-cep)
5. [Frontend — Tecnologias e Componentes](#5-frontend--tecnologias-e-componentes)
   - 5.1 [React 19 com Vite 8](#51-react-19-com-vite-8)
   - 5.2 [TypeScript 5](#52-typescript-5)
   - 5.3 [TanStack Router](#53-tanstack-router)
   - 5.4 [TanStack Query (React Query)](#54-tanstack-query-react-query)
   - 5.5 [React Hook Form com Zod](#55-react-hook-form-com-zod)
   - 5.6 [Tailwind CSS v4](#56-tailwind-css-v4)
   - 5.7 [Axios com Proxy Vite](#57-axios-com-proxy-vite)
   - 5.8 [Arquitetura de Componentes do Cliente](#58-arquitetura-de-componentes-do-cliente)
6. [Infraestrutura e DevOps](#6-infraestrutura-e-devops)
   - 6.1 [Docker e Containers](#61-docker-e-containers)
   - 6.2 [Docker Compose](#62-docker-compose)
   - 6.3 [Multi-stage Build](#63-multi-stage-build)
   - 6.4 [RabbitMQ — Mensageria Assíncrona](#64-rabbitmq--mensageria-assíncrona)
7. [Fluxos de Dados Detalhados](#7-fluxos-de-dados-detalhados)
   - 7.1 [Fluxo: Criar Produto](#71-fluxo-criar-produto)
   - 7.2 [Fluxo: Criar Pedido e Debitar Estoque](#72-fluxo-criar-pedido-e-debitar-estoque)
8. [Como Executar a Solução](#8-como-executar-a-solução)
   - 8.1 [Pré-requisitos](#81-pré-requisitos)
   - 8.2 [Executando com Docker Compose](#82-executando-com-docker-compose)
   - 8.3 [Executando o Cliente React](#83-executando-o-cliente-react)
9. [Endpoints das APIs](#9-endpoints-das-apis)
10. [Variáveis de Ambiente](#10-variáveis-de-ambiente)
11. [Decisões Arquiteturais e Trade-offs](#11-decisões-arquiteturais-e-trade-offs)
12. [Glossário](#12-glossário)

---

## 1. Visão Geral da Solução

Esta solução é um **template de referência** que demonstra como construir um sistema distribuído usando as melhores práticas do ecossistema .NET e React. Ela simula um domínio de comércio eletrônico com dois microserviços independentes:

| Serviço | Responsabilidade | Porta |
|---------|-----------------|-------|
| **Produtos API** | Gerenciamento do catálogo de produtos e controle de estoque | `5001` |
| **Pedidos API** | Criação e consulta de pedidos, orquestração do fluxo de compra e consulta de CEP | `5002` |
| **React Client** | Interface web que consome ambas as APIs e expõe consulta de CEP | `5173` |
| **RabbitMQ** | Broker de mensagens para comunicação assíncrona | `15672` (UI) |

**Diagrama de alto nível:**

```
???????????????????????????????????????????????????????????????
?                     React Client (5173)                     ?
?           TanStack Router + Query + React Hook Form         ?
?????????????????????????????????????????????????????????????-?
           ? HTTP /api/produtos          ? HTTP /api/pedidos
           ?                            ?
????????????????????         ????????????????????????
?   Produtos API   ???????????    Pedidos API        ?
?   (porta 5001)   ? HTTP    ?    (porta 5002)        ?
?                  ? síncrono?                        ?
?  ??????????????  ?         ?  ???????????????????  ?
?  ? SQL Server ?  ?         ?  ?   SQL Server    ?  ?
?  ? ProdutosDb ?  ?         ?  ?   PedidosDb     ?  ?
?  ??????????????  ?         ?  ???????????????????  ?
????????????????????         ??????????????????????????
           ?                            ? Publica
           ? Consome                    ? PedidoConfirmadoEvent
           ??????????????????????????????
                        ?
               ???????????????????
               ?    RabbitMQ     ?
               ?  (porta 5672)   ?
               ???????????????????
```

---

## 2. Estrutura de Pastas

```
Arquitetura/
??? shared/
?   ??? Arquitetura.SharedKernel/       # Contratos e primitivas compartilhadas
?       ??? Common/
?       ?   ??? Result.cs               # Result Pattern genérico
?       ?   ??? Error.cs                # Representação de erro tipado
?       ??? Exceptions/
?       ?   ??? DomainException.cs      # Exceção base de domínio
?       ??? Messaging/
?       ?   ??? IIntegrationEvent.cs    # Contrato de Integration Event
?       ?   ??? PedidoConfirmadoIntegrationEvent.cs
?       ??? Primitives/
?           ??? Entity.cs               # Classe base com Id e Domain Events
?           ??? IDomainEvent.cs         # Marcador de Domain Event
?
??? services/
?   ??? Produtos/
?   ?   ??? Produtos.Domain/            # Regras de negócio puras
?   ?   ?   ??? Entities/Produto.cs
?   ?   ?   ??? Exceptions/ProdutoException.cs
?   ?   ??? Produtos.Application/       # Casos de uso (CQRS)
?   ?   ?   ??? Abstractions/           # Interfaces de repositório e UoW
?   ?   ?   ??? Features/
?   ?   ?       ??? Commands/           # CriarProduto, AtualizarProduto, DeletarProduto
?   ?   ?       ??? Queries/            # ListarProdutos, ObterProdutoPorId
?   ?   ??? Produtos.Infrastructure/    # Implementações técnicas
?   ?   ?   ??? Data/                   # DbContext, Migrations, Configurations
?   ?   ?   ??? Messaging/              # PedidoConfirmadoConsumer (RabbitMQ)
?   ?   ?   ??? Repositories/          # Implementação do IProdutoRepository
?   ?   ??? Produtos.Api/              # Host HTTP
?   ?       ??? Endpoints/             # Minimal API endpoints
?   ?       ??? Middleware/            # ExceptionHandlingMiddleware
?   ?       ??? Dockerfile
?   ?       ??? Program.cs
?   ?
?   ??? Pedidos/
?       ??? Pedidos.Domain/            # Entidades Pedido e ItemPedido
?       ??? Pedidos.Application/       # CriarPedido, ListarPedidosPorCliente
?       ??? Pedidos.Infrastructure/
?       ?   ??? Http/                  # ProdutosServiceClient (HttpClient tipado)
?       ?   ??? Messaging/             # MassTransitEventPublisher
?       ??? Pedidos.Api/
?
??? arquitetura.client/                # Frontend React
?   ??? src/
?       ??? components/
?       ?   ??? layout/                # RootLayout, Navbar
?       ?   ??? ui/                    # Button, Input, Card, Badge, Spinner...
?       ??? hooks/                     # useProdutos, usePedidos (TanStack Query)
?       ??? lib/                       # http.ts (Axios), utils.ts
?       ??? pages/
?       ?   ??? dashboard/
?       ?   ??? produtos/
?       ?   ??? pedidos/
?       ??? services/                  # produtosService, pedidosService
?       ??? types/                     # Tipagens TypeScript dos DTOs
?       ??? router.ts                  # TanStack Router
?       ??? main.tsx                   # Entry point
?
??? docker-compose.yml
??? .env                               # Variáveis sensíveis (não versionado)
```

---

## 3. Arquiteturas e Padrões Utilizados

### 3.1 Microserviços

**Definição:** Estilo arquitetural onde a aplicação é dividida em serviços pequenos, independentes, cada um com sua própria responsabilidade de negócio, banco de dados e ciclo de deploy.

**Nesta solução:**
- `Produtos` e `Pedidos` são serviços completamente independentes
- Cada um tem seu próprio processo, banco de dados e container Docker
- A comunicação ocorre de duas formas: síncrona (HTTP) e assíncrona (RabbitMQ)
- Um serviço pode ser escalado, atualizado ou reiniciado sem afetar o outro

**Benefícios demonstrados:**
- Isolamento de falhas: se `Pedidos` cair, `Produtos` continua operacional
- Escalabilidade independente: pode-se escalar só a API com maior carga
- Times diferentes podem trabalhar em paralelo em cada serviço

### 3.2 Clean Architecture

**Definição:** Proposta por Robert C. Martin, organiza o código em camadas concêntricas onde as dependências sempre apontam para dentro (em direção ao domínio), garantindo que as regras de negócio não dependam de frameworks, banco de dados ou UI.

**Camadas implementadas por serviço:**

```
????????????????????????????????????????????
?              API (Presentation)          ?  ? Endpoints, Middleware, Dockerfile
????????????????????????????????????????????
?           Application (Use Cases)        ?  ? Commands, Queries, Handlers
????????????????????????????????????????????
?             Domain (Entities)            ?  ? Entidades, Exceções de Domínio
????????????????????????????????????????????
?         Infrastructure (Technical)       ?  ? EF Core, RabbitMQ, HttpClient
????????????????????????????????????????????
```

**Regra de dependência:**
- `Domain` não referencia nenhum outro projeto
- `Application` referencia apenas `Domain` e `SharedKernel`
- `Infrastructure` referencia `Application` (implementa as interfaces)
- `Api` referencia `Application` e `Infrastructure` (apenas para registro de DI)

### 3.3 CQRS com MediatR

**Definição:** Command Query Responsibility Segregation — separação das operações de leitura (Queries) das operações de escrita (Commands). O MediatR atua como mediador, desacoplando quem envia a operação de quem a executa.

**Exemplo — Criar um produto:**

```csharp
// 1. Command: objeto imutável que carrega a intenção
public sealed record CriarProdutoCommand(
    string Nome, string Descricao, decimal Preco, int Estoque)
    : IRequest<Result<Guid>>;

// 2. Handler: executa a lógica do caso de uso
internal sealed class CriarProdutoCommandHandler(
    IProdutoRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CriarProdutoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CriarProdutoCommand request, CancellationToken ct)
    {
        var produto = Produto.Criar(request.Nome, request.Descricao, request.Preco, request.Estoque);
        await repository.AdicionarAsync(produto, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(produto.Id);
    }
}

// 3. Endpoint: apenas envia o command via ISender
private static async Task<IResult> CriarAsync(
    CriarProdutoCommand command, ISender sender, CancellationToken ct)
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.CreatedAtRoute("ObterProduto", new { id = result.Value }, result.Value)
        : Results.BadRequest(result.Error);
}
```

**Pipeline Behavior:** O MediatR permite inserir comportamentos transversais entre o envio e o handling, como validação automática via FluentValidation antes que o handler seja executado.

### 3.4 Domain-Driven Design (DDD)

**Conceitos aplicados:**

| Conceito | Onde está | Descrição |
|----------|-----------|-----------|
| **Entity** | `SharedKernel/Primitives/Entity.cs` | Classe base com identidade (Id) e lista de Domain Events |
| **Aggregate Root** | `Produto.cs`, `Pedido.cs` | Entidades raiz que protegem invariantes de negócio |
| **Factory Method** | `Produto.Criar(...)`, `Pedido.Criar(...)` | Construtores privados, criação apenas via método estático com validação |
| **Domain Exception** | `ProdutoException`, `PedidoException` | Exceções específicas do domínio para violações de regras de negócio |
| **Value Object** | `ItemPedido` (record) | Componente de um Pedido sem identidade própria, imutável |
| **Ubiquitous Language** | Nomes em português | Código usa a mesma linguagem do negócio: `DebitarEstoque`, `Confirmar`, `Cancelar` |

**Exemplo — Invariante protegida na entidade:**

```csharp
// O domínio protege suas regras internamente — ninguém pode criar um Produto inválido
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
```

### 3.5 Result Pattern

**Definição:** Em vez de lançar exceções para controlar fluxos esperados (ex: "produto não encontrado"), os métodos retornam um objeto `Result<T>` que encapsula sucesso ou falha com um `Error` tipado.

**Benefícios:**
- Torna explícito que uma operação pode falhar
- Elimina try/catch desnecessários nos handlers
- Erros de negócio são diferentes de exceções técnicas

```csharp
// Definição
public class Result<TValue> : Result
{
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException(...);
    public bool IsSuccess { get; }
    public Error Error { get; }
}

// Uso no handler
if (produto is null)
    return Result.Failure<Guid>(new Error("Produto.NaoEncontrado", $"Produto não encontrado."));

// Uso no endpoint
var result = await sender.Send(command, ct);
return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
```

### 3.6 Domain Events vs Integration Events

**Domain Events** são eventos que ocorrem dentro dos limites de um único serviço e são processados no mesmo contexto transacional. Nesta solução, a infraestrutura para emiti-los está preparada via `Entity.RaiseDomainEvent()`.

**Integration Events** cruzam fronteiras de serviço via broker de mensagens. São o mecanismo de comunicação assíncrona entre microserviços.

```csharp
// Integration Event — contrato no SharedKernel (compartilhado por todos os serviços)
public interface IIntegrationEvent
{
    Guid EventId { get; }       // Para idempotência
    DateTime OcorridoEm { get; }
}

// Evento concreto publicado por Pedidos e consumido por Produtos
public sealed record PedidoConfirmadoIntegrationEvent(
    Guid EventId,
    DateTime OcorridoEm,
    Guid PedidoId,
    Guid ClienteId,
    IReadOnlyList<ItemPedidoConfirmado> Itens) : IIntegrationEvent;
```

### 3.7 Database per Service

**Definição:** Cada microserviço possui seu próprio banco de dados isolado. Nenhum serviço acessa diretamente o banco do outro.

**Implementado com:**
- `ProdutosDb` — banco exclusivo do serviço Produtos
- `PedidosDb` — banco exclusivo do serviço Pedidos
- Duas instâncias separadas do SQL Server no Docker Compose
- Migrations independentes por serviço

**Consequência:** A comunicação entre os dados de diferentes serviços ocorre apenas via HTTP (síncrono) ou via eventos (assíncrono), nunca por consultas diretas ao banco alheio.

### 3.8 Shared Kernel

**Definição:** Conjunto mínimo de código compartilhado entre os microserviços. Contém apenas o que é realmente universal, evitando acoplamento excessivo.

**Conteúdo do `Arquitetura.SharedKernel`:**
- `Entity` — classe base para entidades de domínio
- `IDomainEvent` — marcador de domain events
- `IIntegrationEvent` — contrato de integration events
- `PedidoConfirmadoIntegrationEvent` — evento concreto compartilhado
- `Result<T>` e `Error` — Result Pattern
- `DomainException` — exceção base

---

## 4. Backend — Tecnologias e Componentes

### 4.1 .NET 10 e ASP.NET Core Minimal APIs

O .NET 10 é a plataforma de runtime e o ASP.NET Core é o framework web. As **Minimal APIs** (introduzidas no .NET 6 e refinadas no .NET 10) permitem definir endpoints HTTP com código mínimo de cerimônia, sem Controllers.

```csharp
// Program.cs — startup completo e enxuto
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();       // MediatR, Validators, Behaviors
builder.Services.AddInfrastructureServices(builder.Configuration); // EF Core, RabbitMQ
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();       // Migrations automáticas na startup

app.UseMiddleware<ExceptionHandlingMiddleware>(); // Tratamento global de erros

app.MapOpenApi();
app.MapScalarApiReference(opt => opt.WithTitle("Produtos API"));
app.MapProdutosEndpoints();                       // Feature-based endpoint registration
app.MapHealthChecks("/health");

app.Run();
```

**Endpoints com MapGroup:**

```csharp
var group = app.MapGroup("/api/produtos").WithTags("Produtos");

group.MapGet("/",           ListarAsync)        .WithSummary("Lista todos os produtos");
group.MapGet("/{id:guid}",  ObterPorIdAsync)    .WithName("ObterProduto");
group.MapPost("/",          CriarAsync)         .WithSummary("Cria um novo produto");
group.MapPut("/{id:guid}",  AtualizarAsync)     .WithSummary("Atualiza um produto");
group.MapDelete("/{id:guid}", DeletarAsync)     .WithSummary("Remove um produto");
```

### 4.2 Entity Framework Core 10

ORM (Object-Relational Mapper) da Microsoft que permite trabalhar com bancos de dados usando objetos C# e LINQ, gerando SQL automaticamente.

**DbContext com Fluent API:**

```csharp
// ProdutosDbContext implementa também IUnitOfWork
public class ProdutosDbContext(DbContextOptions<ProdutosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Produto> Produtos => Set<Produto>();

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => base.SaveChangesAsync(ct);
}
```

**Configuração de entidade (Fluent API):**

```csharp
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Preco).HasPrecision(18, 2);
    }
}
```

**Migrations automáticas na startup:**
```csharp
// Aplicado antes de iniciar o servidor HTTP
await app.Services.ApplyMigrationsAsync();
```

### 4.3 MediatR 14

Biblioteca que implementa o padrão Mediator, desacoplando quem emite uma operação (endpoint) de quem a processa (handler).

**Registro (sem reflection explícita — padrão nativo do MediatR 14):**

```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));
```

**Pipeline Behavior para Validação:**

```csharp
// Intercepta toda requisição MediatR antes do handler
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(); // Prossegue para o handler
    }
}
```

### 4.4 FluentValidation 12

Biblioteca para definição de regras de validação de forma fluente e legível, desacopladas das entidades de domínio.

```csharp
public class CriarProdutoCommandValidator : AbstractValidator<CriarProdutoCommand>
{
    public CriarProdutoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("Mínimo 3 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0).WithMessage("Estoque não pode ser negativo.");
    }
}
```

A validação é acionada automaticamente pelo `ValidationBehavior` do MediatR antes que qualquer handler seja executado.

### 4.5 MassTransit 8 com RabbitMQ

MassTransit é uma abstração de mensageria que suporta vários brokers (RabbitMQ, Azure Service Bus, Amazon SQS). O RabbitMQ é o broker open-source de mensagens que implementa o protocolo AMQP.

**Fluxo assíncrono entre serviços:**

```
Pedidos API          RabbitMQ             Produtos API
    ?                   ?                     ?
    ???Publish Event??????                     ?
    ?  PedidoConfirmado  ???Deliver Message?????
    ?                   ?                     ???DebitarEstoque()
    ?                   ?                     ???SaveChanges()
```

**Publisher (Pedidos.Infrastructure):**

```csharp
internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, ct);
}
```

**Consumer (Produtos.Infrastructure):**

```csharp
internal sealed class PedidoConfirmadoConsumer(
    IProdutoRepository produtoRepository,
    IUnitOfWork unitOfWork,
    ILogger<PedidoConfirmadoConsumer> logger) : IConsumer<PedidoConfirmadoIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PedidoConfirmadoIntegrationEvent> context)
    {
        foreach (var item in context.Message.Itens)
        {
            var produto = await produtoRepository.ObterPorIdAsync(item.ProdutoId, context.CancellationToken);
            produto?.DebitarEstoque(item.Quantidade);
        }
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
```

**Garantias do MassTransit com RabbitMQ:**
- **At-least-once delivery**: a mensagem é entregue pelo menos uma vez
- **Retry automático**: em caso de falha no consumer, a mensagem é reprocessada
- **Dead-letter queue**: mensagens que falham repetidamente são movidas para uma fila especial

### 4.6 Scalar — Interface de API no Browser

Scalar é uma alternativa moderna ao Swagger UI, integrada nativamente com o sistema de OpenAPI do .NET 10. Gera uma interface interativa e moderna para explorar e testar endpoints diretamente no browser.

```csharp
app.MapOpenApi();  // Gera o JSON da spec OpenAPI em /openapi/v1.json
app.MapScalarApiReference(opt => opt.WithTitle("Produtos API"));  // UI em /scalar/v1
```

- Acesse `http://localhost:5001/scalar/v1` para a API de Produtos
- Acesse `http://localhost:5002/scalar/v1` para a API de Pedidos

### 4.7 HttpClient Tipado — Comunicação entre Serviços

Para o caso síncrono onde Pedidos precisa obter informações de um Produto em tempo real (antes de criar o pedido), usa-se um `HttpClient` tipado gerenciado pelo `IHttpClientFactory` do ASP.NET Core.

```csharp
// Interface na camada Application (sem dependência de infraestrutura)
public interface IProdutosServiceClient
{
    Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default);
}

// Implementação na camada Infrastructure
internal sealed class ProdutosServiceClient(HttpClient httpClient) : IProdutosServiceClient
{
    public async Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<ProdutoDto>($"api/produtos/{produtoId}", ct);
}

// Registro com baseAddress configurável por ambiente
services.AddHttpClient<IProdutosServiceClient, ProdutosServiceClient>(client =>
    client.BaseAddress = new Uri(configuration["Services:ProdutosUrl"]!));
```

**Benefícios do IHttpClientFactory:**
- Gerencia o pool de `HttpMessageHandler`, evitando socket exhaustion
- Facilita testes (pode-se mockar `IProdutosServiceClient`)
- BaseAddress configurável por ambiente (local vs. Docker vs. produção)

### 4.8 Health Checks

Endpoints `/health` expostos por cada API para que o Docker Compose e orquestradores (Kubernetes, etc.) possam verificar se o serviço está saudável antes de rotear tráfego.

```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

No `docker-compose.yml`, o `depends_on` com `condition: service_healthy` garante que a API de Pedidos só inicie após a API de Produtos estar saudável:

```yaml
depends_on:
  produtos-api:
    condition: service_healthy
```

### 4.9 Middleware de Tratamento de Exceções

Um middleware centralizado intercepta todas as exceções não tratadas e retorna respostas padronizadas no formato `application/problem+json` (RFC 7807).

```csharp
// Hierarquia de tratamento:
// ValidationException (FluentValidation) ? 422 Unprocessable Entity
// DomainException (regra de negócio)     ? 400 Bad Request
// Exception (erro técnico inesperado)    ? 500 Internal Server Error
public async Task InvokeAsync(HttpContext context)
{
    try { await next(context); }
    catch (ValidationException ex)
    {
        await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity,
            "Erro de Validação", ex.Errors.Select(e => e.ErrorMessage));
    }
    catch (DomainException ex)
    {
        await WriteResponseAsync(context, HttpStatusCode.BadRequest,
            "Regra de Negócio", [ex.Message]);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro inesperado");
        await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
            "Erro Interno", ["Ocorreu um erro inesperado."]);
    }
}
```

### 4.10 ViaCEP — Integração com API Pública de CEP

A Pedidos API expõe um endpoint de consulta de CEP que internamente consome a API pública [ViaCEP](https://viacep.com.br). Essa funcionalidade segue o mesmo padrão arquitetural do restante do serviço: interface na Application, implementação na Infrastructure, CQRS via MediatR e Result Pattern.

**Interface (Pedidos.Application):**
```csharp
public interface IViaCepClient
{
    Task<ViaCepDto?> ConsultarCepAsync(string cep, CancellationToken ct = default);
}
```

**Implementação (Pedidos.Infrastructure):**
```csharp
internal sealed class ViaCepClient(HttpClient httpClient) : IViaCepClient
{
    public async Task<ViaCepDto?> ConsultarCepAsync(string cep, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"ws/{cep}/json/", ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = await response.Content.ReadFromJsonAsync<ViaCepResponse>(ct);
        return data?.Erro == true ? null : data?.ToDto();
    }
}
// Registro: services.AddHttpClient<IViaCepClient, ViaCepClient>(c =>
//     c.BaseAddress = new Uri("https://viacep.com.br/"));
```

**Endpoint (Pedidos.Api):**
```csharp
// GET /api/cep/{cep}
group.MapGet("/{cep}", async (string cep, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new ConsultarCepQuery(cep), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
})
.WithName("ConsultarCep");
```

**Decisão de design:** A consulta de CEP vive no serviço Pedidos porque é diretamente relevante ao preenchimento do endereço de entrega. Evita criar um microserviço dedicado para esse proxy simples.

---

## 5. Frontend — Tecnologias e Componentes

### 5.1 React 19 com Vite 8

**React 19** é a versão mais recente da biblioteca de UI declarativa da Meta. O **Vite 8** é o bundler e dev-server moderno que usa ES modules nativos no browser durante o desenvolvimento, tornando o hot reload praticamente instantâneo.

```bash
# Estrutura de scripts
npm run dev      # Inicia dev server com HMR (Hot Module Replacement)
npm run build    # Build de produção com TypeScript check + Vite bundler
npm run preview  # Serve o build de produção localmente
```

### 5.2 TypeScript 5

TypeScript adiciona tipagem estática ao JavaScript, capturando erros em tempo de compilação. Toda a base de código do cliente é em TypeScript com configurações estritas.

**Tipos dos DTOs da API:**

```typescript
// src/types/produto.ts
export interface Produto {
  id: string;
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
  criadoEm: string;
  atualizadoEm?: string;
}

// src/types/pedido.ts
export type StatusPedidoNumerico = 0 | 1 | 2; // Pendente | Confirmado | Cancelado
export interface Pedido {
  id: string;
  status: StatusPedidoNumerico;
  total: number;
  itens?: ItemPedidoDetalhe[];
  criadoEm: string;
}

// src/types/cep.ts
export interface ViaCep {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  localidade: string;
  uf: string;
  ibge: string;
  gia: string;
  ddd: string;
  siafi: string;
}
```

**Alias de path**
```typescript
// Importação limpa sem caminhos relativos ../../
import { Button } from '@/components/ui/Button';
import { useProdutos } from '@/hooks/useProdutos';
```

### 5.3 TanStack Router

Roteador type-safe para React, com suporte a layout aninhados, parâmetros tipados e navegação programática. Difere do React Router por ser completamente tipado — rotas inexistentes geram erro em tempo de compilação.

```typescript
// src/router.ts — definição manual do árore de rotas
const rootRoute = createRootRoute({ component: RootLayout });

const produtosRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/produtos',
  component: ProdutosPage,
});

const editarProdutoRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/produtos/$id/editar',   // $id é um parâmetro tipado
  component: EditarProdutoPage,
});

const routeTree = rootRoute.addChildren([
  indexRoute, produtosRoute, novoProdutoRoute, editarProdutoRoute,
  pedidosRoute, novoPedidoRoute, cepRoute,
]);

export const router = createRouter({ routeTree });
```

**Navegação programática:**
```typescript
const navigate = useNavigate();
navigate({ to: '/produtos' });           // navegação simples
navigate({ to: '/produtos/$id/editar', params: { id: produto.id } }); // com params
```

### 5.4 TanStack Query (React Query)

Biblioteca de gerenciamento de estado de servidor — cuida do cache, sincronização, revalidação automática, loading/error states e mutations para dados que vêm de APIs.

**Query Keys (cache granular):**
```typescript
export const produtosKeys = {
  all: ['produtos'] as const,
  detail: (id: string) => ['produtos', id] as const,
};
```

**Hook de listagem com cache:**
```typescript
export function useProdutos() {
  return useQuery({
    queryKey: produtosKeys.all,
    queryFn: produtosService.listar,
    staleTime: 1000 * 30,  // dados frescos por 30 segundos
  });
}
```

**Mutation com invalidação de cache:**
```typescript
export function useCriarProduto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: produtosService.criar,
    onSuccess: () => {
      // Invalida o cache da lista — próximo acesso buscará dados atualizados
      queryClient.invalidateQueries({ queryKey: produtosKeys.all });
    },
  });
}
```

**Estados disponíveis automaticamente:**
```typescript
const { data, isLoading, isFetching, isError, error } = useProdutos();
```

**React Query DevTools** (visível no canto inferior da tela em desenvolvimento):
- Inspeciona todas as queries e seu estado atual
- Visualiza dados em cache
- Força refetch ou invalida queries manualmente
- Removido automaticamente em builds de produção

### 5.5 React Hook Form com Zod

**React Hook Form** é uma biblioteca de formulários performática que evita re-renders desnecessários ao usar refs nativas do DOM em vez de estado React controlado. **Zod** é uma biblioteca de validação de schemas com inferência de tipos TypeScript.

**Schema de validação com Zod:**
```typescript
const schema = z.object({
  nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  descricao: z.string().min(5, 'Descrição deve ter pelo menos 5 caracteres'),
  preco: z.number().positive('Preço deve ser positivo'),
  estoque: z.number().int().min(0, 'Estoque não pode ser negativo'),
});

// O tipo TypeScript é INFERIDO automaticamente do schema Zod
type FormData = z.infer<typeof schema>;
```

**Integração com zodResolver:**
```typescript
const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
  resolver: zodResolver(schema),  // conecta Zod como validador do RHF
});

const onSubmit = (data: FormData) => {
  // data já está tipado e validado
  criar.mutate(data, {
    onSuccess: () => navigate({ to: '/produtos' }),
  });
};
```

**Campos com conversão de tipo:**
```typescript
// O input HTML sempre retorna string; valueAsNumber converte para number
{...register('preco', { valueAsNumber: true })}
{...register('estoque', { valueAsNumber: true })}
```

### 5.6 Tailwind CSS v4

Framework CSS utility-first. Na versão 4, a configuração migrou de `tailwind.config.js` para dentro do próprio CSS, e o plugin do Vite é a forma recomendada de integração.

**Configuração v4:**
```css
/* src/index.css */
@import 'tailwindcss';  /* substitui os três @tailwind directives da v3 */
```

```typescript
// vite.config.ts
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
});
```

**Utilitário `cn()` para classes condicionais:**
```typescript
// src/lib/utils.ts — combina clsx + tailwind-merge
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Uso em componentes
<button className={cn(
  'px-4 py-2 rounded-md font-medium',
  variant === 'primary' && 'bg-indigo-600 text-white',
  variant === 'outline' && 'border border-gray-300 text-gray-700',
  disabled && 'opacity-50 cursor-not-allowed',
)} />
```

### 5.7 Axios com Proxy Vite

**Axios** é o cliente HTTP para as chamadas à API. Durante o desenvolvimento, o **Proxy do Vite** redireciona requisições para as APIs backend, evitando erros de CORS.

```typescript
// src/lib/http.ts — três instâncias isoladas com baseURL distintas
export const produtosApi = axios.create({
  baseURL: '/api/produtos',
  headers: { 'Content-Type': 'application/json' },
});

export const pedidosApi = axios.create({
  baseURL: '/api/pedidos',
  headers: { 'Content-Type': 'application/json' },
});

export const cepApi = axios.create({
  baseURL: '/api/cep',
  headers: { 'Content-Type': 'application/json' },
});
```

```typescript
// vite.config.ts — proxy transparente em desenvolvimento
server: {
  proxy: {
    '/api/produtos': { target: 'http://localhost:5001', changeOrigin: true },
    '/api/pedidos':  { target: 'http://localhost:5002', changeOrigin: true },
    '/api/cep':      { target: 'http://localhost:5002', changeOrigin: true },
  }
}
```

> **Nota:** `/api/cep` é roteado para a **Pedidos API** (porta 5002), que internamente consulta `https://viacep.com.br` e retorna os dados de endereço. O frontend nunca acessa o ViaCEP diretamente.

**Como funciona o proxy:**
```
Browser                   Vite Dev Server           Backend
  ?                            ?                       ?
  ???GET /api/produtos/??????????                       ?
  ?                            ???GET /api/produtos/?????
  ?                            ????200 JSON??????????????
  ????200 JSON??????????????????                       ?
```

### 5.8 Arquitetura de Componentes do Cliente

O frontend segue uma arquitetura em camadas:

```
pages/           -> Composição de funcionalidades completas (telas)
  |  usa
hooks/           -> Estado de servidor (TanStack Query) e lógica de negócio UI
  |  usa
services/        -> Chamadas HTTP puras (funções async que chamam Axios)
  |  usa
lib/http.ts      -> produtosApi | pedidosApi | cepApi

components/ui/   -> Componentes reutilizáveis sem lógica de negócio
  Button, Input, Card, Badge, Spinner, EmptyState

components/layout/ -> Shell da aplicação
  RootLayout, Navbar
```

**Páginas implementadas:**

| Página | Rota | Responsabilidade |
|--------|------|------------------|
| `DashboardPage` | `/` | Visão geral com cards de resumo |
| `ProdutosPage` | `/produtos` | Listagem com filtro e ações |
| `NovoProdutoPage` | `/produtos/novo` | Formulário de criação |
| `EditarProdutoPage` | `/produtos/$id/editar` | Formulário de edição |
| `PedidosPage` | `/pedidos` | Listagem de pedidos por cliente |
| `NovoPedidoPage` | `/pedidos/novo` | Formulário de criação de pedido |
| `ConsultaCepPage` | `/cep` | Consulta de endereço por CEP via ViaCEP |

**Componentes UI criados:**

| Componente | Responsabilidade |
|-----------|-----------------|
| `Button` | Botão com variantes (primary, outline, ghost, secondary), estado de loading com spinner, forwarded ref |
| `Input` | Campo de texto com label integrado, mensagem de erro, forwarded ref |
| `Card` | Contêiner de conteúdo com `CardHeader`, `CardTitle`, `CardContent` |
| `Badge` | Etiqueta de status com variantes de cor (default, success, warning, destructive) |
| `Spinner` / `PageSpinner` | Indicador de carregamento inline e de página inteira |
| `EmptyState` | Estado vazio com título, descrição e ação opcional |

---

## 6. Infraestrutura e DevOps

### 6.1 Docker e Containers

Cada microserviço é empacotado em um container Docker independente. Os containers isolam o ambiente de execução, garantindo que "funciona na minha máquina" seja irrelevante.

**Imagens base utilizadas:**
- `mcr.microsoft.com/dotnet/sdk:10.0` — SDK completo para build
- `mcr.microsoft.com/dotnet/aspnet:10.0` — Runtime mínimo para execução
- `mcr.microsoft.com/mssql/server:2022-latest` — SQL Server Express
- `rabbitmq:3.13-management` — RabbitMQ com painel de gerenciamento

**Segurança:** Os containers de API executam com usuário não-root (`$APP_UID`), fornecido nativamente pelas imagens oficiais da Microsoft para .NET.

### 6.2 Docker Compose

O `docker-compose.yml` orquestra todos os 5 containers com suas dependências, redes e volumes.

**Ordem de inicialização com healthchecks:**
```
sqlserver-produtos ??healthy??? produtos-api ??healthy???
sqlserver-pedidos  ??healthy???                          ???? pedidos-api
rabbitmq           ??healthy???????????????????????????????
```

**Rede interna:**
```yaml
networks:
  arquitetura-net:
    driver: bridge  # rede virtual isolada onde todos os containers se comunicam
```

**Volumes persistentes:**
```yaml
volumes:
  sqlserver-produtos-data:  # dados do SQL Server sobrevivem ao restart
  sqlserver-pedidos-data:
  rabbitmq-data:
```

### 6.3 Multi-stage Build

Técnica Docker para separar o ambiente de build do ambiente de execução, resultando em imagens de produção menores e mais seguras.

```dockerfile
# ?? STAGE 1: Build ??????????????????????????????????????????
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os .csproj primeiro (aproveitamento de cache de layers)
COPY shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj shared/Arquitetura.SharedKernel/
COPY services/Produtos/Produtos.Api/Produtos.Api.csproj services/Produtos/Produtos.Api/
# ... demais projetos

RUN dotnet restore services/Produtos/Produtos.Api/Produtos.Api.csproj

COPY shared/ shared/
COPY services/Produtos/ services/Produtos/

RUN dotnet publish services/Produtos/Produtos.Api/Produtos.Api.csproj \
    -c Release -o /app/publish --no-restore

# ?? STAGE 2: Runtime ?????????????????????????????????????????
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*  # curl para healthcheck; limpeza reduz tamanho

COPY --from=build /app/publish .  # Copia apenas o output compilado

USER $APP_UID  # Execução com usuário não-root
EXPOSE 8080
ENTRYPOINT ["dotnet", "Produtos.Api.dll"]
```

**Resultado:** A imagem final contém apenas o ASP.NET Core runtime + binários publicados, sem SDK, código-fonte ou arquivos temporários de build.

### 6.4 RabbitMQ — Mensageria Assíncrona

RabbitMQ implementa o protocolo AMQP (Advanced Message Queuing Protocol) e fornece filas de mensagens duráveis entre os serviços.

**Painel de gerenciamento:** `http://localhost:15672` (usuário: `guest`, senha: `guest`)

**Como visualizar mensagens publicadas:**
1. Acesse o painel em `http://localhost:15672`
2. Vá em **Queues and Streams** para ver as filas criadas pelo MassTransit
3. Clique em uma fila ? **Get messages** para inspecionar mensagens
4. Vá em **Exchanges** para ver os exchanges de publicação

**Nomenclatura automática do MassTransit:** O MassTransit cria automaticamente exchanges e filas baseados no nome do tipo do evento (`PedidoConfirmadoIntegrationEvent`).

---

## 7. Fluxos de Dados Detalhados

### 7.1 Fluxo: Criar Produto

```
React Client                 Produtos API              SQL Server
     ?                            ?                        ?
     ???POST /api/produtos ?????????                        ?
     ?  { nome, descricao,         ?                        ?
     ?    preco, estoque }         ?                        ?
     ?                            ??? ValidationBehavior   ?
     ?                            ?   (FluentValidation)   ?
     ?                            ?                        ?
     ?                            ??? CriarProdutoHandler  ?
     ?                            ?   Produto.Criar(...)   ?
     ?                            ?   repository.Add(...)  ?
     ?                            ???INSERT INTO Produtos????
     ?                            ????OK????????????????????
     ?                            ?   unitOfWork.Save()    ?
     ?                            ?                        ?
     ????201 Created ??????????????                        ?
     ?   Location: /api/produtos/{id}                      ?
```

### 7.2 Fluxo: Criar Pedido e Debitar Estoque

```
React         Pedidos API      Produtos API     RabbitMQ    Produtos API
Client            ?                ?               ?          (Consumer)
  ?               ?                ?               ?              ?
  ???POST ??????????                ?               ?              ?
  ?  /api/pedidos  ?                ?               ?              ?
  ?                ???GET produto????               ?              ?
  ?                ????ProdutoDto????               ?              ?
  ?                ?                ?               ?              ?
  ?                ? Pedido.Criar() ?               ?              ?
  ?                ? AdicionarItem()?               ?              ?
  ?                ? Confirmar()    ?               ?              ?
  ?                ???INSERT Pedido?????????????????????????????????
  ?                ???Publish Event???????????????????              ?
  ?                ?  PedidoConfirmado               ???Deliver??????
  ????201 Created???                ?               ?  ?           ?
  ?                ?                ?               ?  ? DebitarEstoque()
  ?                ?                ?               ?  ? SaveChanges()
  ?                ?                ?               ???Ack???????????
```

**Ponto-chave:** O cliente recebe a resposta `201 Created` imediatamente após a persistência do pedido. O débito de estoque acontece de forma assíncrona, desacoplando a latência do pedido do processamento do estoque.

### 7.3 Fluxo: Consultar CEP

```
React Client          Pedidos API (5002)         ViaCEP (externo)
     |                      |                          |
     +--GET /api/cep/01310100+                          |
     |                      +--ConsultarCepQuery--------+
     |                      |  (MediatR)                |
     |                      +--GET ws/01310100/json/---->|
     |                      |<--{ logradouro, bairro... }|
     |                      |                          |
     |    Result.Success     |                          |
     |<--200 OK (ViaCepDto)--+                          |
     |                      |                          |
     |  (CEP inexistente)    |                          |
     +--GET /api/cep/00000000+                          |
     |                      +--GET ws/00000000/json/---->|
     |                      |<--{ "erro": true }---------|
     |<--404 Not Found-------+                          |
```

**Ponto-chave:** O frontend nunca acessa `viacep.com.br` diretamente. A Pedidos API atua como proxy, centralizando o tratamento de erros (CEP inválido, timeout) e mantendo o frontend desacoplado de APIs externas.

---

## 8. Como Executar a Solução

### 8.1 Pré-requisitos

| Ferramenta | Versão mínima | Finalidade |
|-----------|--------------|-----------|
| Docker Desktop | 4.x | Orquestração de containers |
| Node.js | 20 LTS | Frontend React |
| .NET SDK | 10.0 | Desenvolvimento local do backend |

### 8.2 Executando com Docker Compose

**1. Criar o arquivo de variáveis de ambiente:**
```bash
# Na raiz do repositório (onde está o docker-compose.yml)
cp .env.example .env
```

Edite o `.env` com valores seguros:
```env
SA_PASSWORD=SuaSenhaForte@123
RABBITMQ_USER=admin
RABBITMQ_PASS=admin123
```

**2. Construir e iniciar todos os containers:**
```bash
docker-compose up --build
```

**3. Aguardar todos os serviços ficarem saudáveis:**
```bash
docker ps  # Verifique a coluna STATUS — deve mostrar "(healthy)"
```

**4. Verificar os logs de um serviço:**
```bash
docker logs produtos-api --follow
docker logs pedidos-api  --follow
docker logs rabbitmq     --follow
```

**5. URLs disponíveis após o startup:**

| Serviço | URL | Descrição |
|---------|-----|-----------|
| Produtos API | http://localhost:5001/scalar/v1 | Interface interativa Scalar |
| Pedidos API | http://localhost:5002/scalar/v1 | Interface interativa Scalar |
| RabbitMQ UI | http://localhost:15672 | Painel de gerenciamento |
| Produtos Health | http://localhost:5001/health | Health check |
| Pedidos Health | http://localhost:5002/health | Health check |

### 8.3 Executando o Cliente React

Em um terminal separado:

```bash
cd arquitetura.client
npm install       # instalar dependências (apenas na primeira vez)
npm run dev       # iniciar dev server em http://localhost:5173
```

O cliente já está configurado com proxy para as APIs. Certifique-se de que as APIs estão rodando antes de acessar o cliente.

---

## 9. Endpoints das APIs

### Produtos API — `http://localhost:5001`

| Método | Rota | Descrição | Body |
|--------|------|-----------|------|
| `GET` | `/api/produtos` | Lista todos os produtos | — |
| `GET` | `/api/produtos/{id}` | Obtém produto por Id | — |
| `POST` | `/api/produtos` | Cria novo produto | `{ nome, descricao, preco, estoque }` |
| `PUT` | `/api/produtos/{id}` | Atualiza produto | `{ nome, descricao, preco, estoque }` |
| `DELETE` | `/api/produtos/{id}` | Remove produto | — |
| `GET` | `/health` | Health check | — |

### Pedidos API — `http://localhost:5002`

| Método | Rota | Descrição | Body |
|--------|------|-----------|------|
| `GET` | `/api/pedidos/cliente/{clienteId}` | Lista pedidos por cliente | — |
| `POST` | `/api/pedidos` | Cria e confirma pedido | `{ clienteId, itens: [{ produtoId, quantidade }] }` |
| `GET` | `/api/cep/{cep}` | Consulta endereço por CEP (via ViaCEP) | — |
| `GET` | `/health` | Health check | — |

**Exemplo de payload para criar pedido:**
```json
{
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "itens": [
    { "produtoId": "ee74c967-144a-4879-8dcb-82608f3e71f2", "quantidade": 2 }
  ]
}
```

**Exemplo de resposta para consultar CEP (`GET /api/cep/01310100`):**
```json
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

---

## 10. Variáveis de Ambiente

### Backend (via `docker-compose.yml` e `.env`)

| Variável | Serviço | Descrição |
|---------|---------|-----------|
| `SA_PASSWORD` | SQL Server | Senha do usuário `sa` |
| `ConnectionStrings__ProdutosConnection` | produtos-api | Connection string do SQL Server Produtos |
| `ConnectionStrings__PedidosConnection` | pedidos-api | Connection string do SQL Server Pedidos |
| `RabbitMQ__Host` | ambas as APIs | Hostname do RabbitMQ (`rabbitmq` na rede Docker) |
| `RabbitMQ__Username` | ambas as APIs | Usuário do RabbitMQ |
| `RabbitMQ__Password` | ambas as APIs | Senha do RabbitMQ |
| `Services__ProdutosUrl` | pedidos-api | URL base da API de Produtos para comunicação interna |
| `ASPNETCORE_ENVIRONMENT` | ambas as APIs | `Development` ou `Production` |

### Frontend (Vite)

O Vite usa o proxy configurado em `vite.config.ts`. Não há variáveis de ambiente sensíveis no cliente em desenvolvimento. Para produção, configure a URL base das APIs em `src/lib/http.ts`.

---

## 11. Decisões Arquiteturais e Trade-offs

| Decisão | Justificativa | Trade-off |
|---------|--------------|-----------|
| **Microserviços desde o início** | Template demonstra a arquitetura completa | Maior complexidade operacional que um monólito |
| **Database per Service** | Independência verdadeira entre serviços | Sem joins entre bancos; requer sincronização via eventos |
| **Comunicação síncrona + assíncrona** | HTTP para dados em tempo real; eventos para side effects | Consistência eventual no estoque (debita após confirmação) |
| **Shared Kernel pequeno** | Evita acoplamento excessivo entre serviços | Duplicação aceitável de código em cada serviço |
| **MassTransit 8.5.9 (não 9.x)** | Versão 9+ requer licença comercial para alguns recursos | Versão 8 é open-source completa e estável |
| **Scalar em vez de Swagger UI** | Interface mais moderna, nativo com .NET 10 OpenAPI | Menor adoção no mercado (mas crescendo) |
| **Minimal APIs em vez de Controllers** | Menos código de cerimônia, mais performance | Menor familiaridade para desenvolvedores acostumados ao MVC |
| **TanStack Router em vez de React Router** | Type-safety completo nas rotas | API mais verbosa para definição manual |
| **Result Pattern em vez de Exceptions** | Fluxos de negócio esperados não são exceções | Necessário propagar `Result` por toda a cadeia de chamadas |
| **CEP no serviço Pedidos** | CEP é diretamente relevante ao endereço de entrega de um pedido | Acoplamento leve de responsabilidade; seria um microserviço excessivo separado |
| **Frontend como proxy para ViaCEP** | Centraliza tratamento de erros e evita CORS no browser | Adiciona latência extra de um hop; mitigado por `staleTime` longo no TanStack Query |

---

## 12. Glossário

| Termo | Definição |
|-------|-----------|
| **Aggregate Root** | Entidade raiz que controla o acesso e as invariantes de um grupo de objetos relacionados |
| **AMQP** | Advanced Message Queuing Protocol — protocolo de mensageria usado pelo RabbitMQ |
| **At-least-once delivery** | Garantia de que uma mensagem é entregue pelo menos uma vez, podendo ser entregue mais |
| **Clean Architecture** | Organização do código em camadas concêntricas com dependências apontando para o domínio |
| **CQRS** | Command Query Responsibility Segregation — separação de leituras e escritas |
| **Consumer** | Componente que lê e processa mensagens de uma fila do RabbitMQ |
| **DDD** | Domain-Driven Design — metodologia de modelagem centrada no domínio de negócio |
| **Dead-letter queue** | Fila especial que armazena mensagens que falharam repetidamente |
| **DTOs** | Data Transfer Objects — objetos usados para transportar dados entre camadas |
| **Exchange** | Componente do RabbitMQ que recebe mensagens publicadas e as roteia para filas |
| **Health Check** | Endpoint que verifica se um serviço está operacional |
| **HMR** | Hot Module Replacement — atualização de módulos sem recarregar a página |
| **HttpClient tipado** | Instância de HttpClient associada a uma interface, gerenciada pelo IHttpClientFactory |
| **Idempotência** | Propriedade de uma operação que pode ser executada múltiplas vezes com o mesmo resultado |
| **Integration Event** | Evento que cruza fronteiras de microserviços via broker de mensagens |
| **Invariante** | Regra de negócio que deve ser sempre verdadeira para um objeto |
| **MediatR** | Biblioteca que implementa o padrão Mediator para desacoplar emissores de handlers |
| **Mediator Pattern** | Padrão que centraliza a comunicação entre objetos através de um intermediário |
| **Minimal API** | Estilo de API do ASP.NET Core com mínima cerimônia de código |
| **Multi-stage build** | Técnica Docker de separar build de runtime para imagens menores |
| **Pipeline Behavior** | Interceptor no pipeline do MediatR executado antes/depois de handlers |
| **Publisher** | Componente que envia mensagens para um exchange do RabbitMQ |
| **Result Pattern** | Retorno explícito de sucesso ou falha sem uso de exceções para fluxos esperados |
| **Shared Kernel** | Código compartilhado mínimo entre microserviços sem criar acoplamento excessivo |
| **Ubiquitous Language** | Linguagem comum entre desenvolvedores e especialistas de domínio, refletida no código |
| **Unit of Work** | Padrão que agrupa operações de banco em uma única transação |
| **Value Object** | Objeto do domínio sem identidade própria, definido apenas por seus atributos |
| **ViaCEP** | API pública brasileira que retorna dados de endereço a partir de um CEP |

---

*Este template foi construído com fins educacionais e de referência arquitetural. Cada decisão foi tomada visando demonstrar as melhores práticas do ecossistema .NET 10 e React 19 em um cenário realista de microserviços.*
