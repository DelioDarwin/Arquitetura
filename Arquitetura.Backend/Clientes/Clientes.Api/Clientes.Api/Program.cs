using Clientes.Infrastructure;
using Clientes.Api.Endpoints;
using Clientes.Api.Middleware;
using Clientes.Application;
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
app.MapScalarApiReference(opt => opt.WithTitle("Clientes API"));

app.MapClientesEndpoints();
app.MapHealthChecks("/health");

app.Run();
