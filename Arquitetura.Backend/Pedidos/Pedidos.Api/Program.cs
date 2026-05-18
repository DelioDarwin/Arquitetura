using Pedidos.Api.Endpoints;
using Pedidos.Api.Middleware;
using Pedidos.Application;
using Pedidos.Infrastructure;
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
app.MapScalarApiReference(opt => opt.WithTitle("Pedidos API"));

app.MapPedidosEndpoints();
app.MapCepEndpoints();
app.MapHealthChecks("/health");

app.Run();
