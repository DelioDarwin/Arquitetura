# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Arquitetura.Shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj Arquitetura.Shared/Arquitetura.SharedKernel/
COPY Arquitetura.Backend/Pedidos/Pedidos.Domain/Pedidos.Domain.csproj Arquitetura.Backend/Pedidos/Pedidos.Domain/
COPY Arquitetura.Backend/Pedidos/Pedidos.Application/Pedidos.Application.csproj Arquitetura.Backend/Pedidos/Pedidos.Application/
COPY Arquitetura.Backend/Pedidos/Pedidos.Infrastructure/Pedidos.Infrastructure.csproj Arquitetura.Backend/Pedidos/Pedidos.Infrastructure/
COPY Arquitetura.Backend/Pedidos/Pedidos.Api/Pedidos.Api.csproj Arquitetura.Backend/Pedidos/Pedidos.Api/

RUN dotnet restore Arquitetura.Backend/Pedidos/Pedidos.Api/Pedidos.Api.csproj

COPY Arquitetura.Shared/ Arquitetura.Shared/
COPY Arquitetura.Backend/Pedidos/ Arquitetura.Backend/Pedidos/

RUN dotnet publish Arquitetura.Backend/Pedidos/Pedidos.Api/Pedidos.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl necessario para o healthcheck do Docker Compose
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Usuario nao-root ja incluso nas imagens oficiais .NET da Microsoft
USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "Pedidos.Api.dll"]

