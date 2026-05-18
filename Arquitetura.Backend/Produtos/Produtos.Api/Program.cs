using Produtos.Api.Endpoints;
using Produtos.Api.Middleware;
using Produtos.Application;
using Produtos.Infrastructure;
using Scalar.AspNetCore;

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

