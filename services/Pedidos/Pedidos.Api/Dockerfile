# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj shared/Arquitetura.SharedKernel/
COPY services/Pedidos/Pedidos.Domain/Pedidos.Domain.csproj services/Pedidos/Pedidos.Domain/
COPY services/Pedidos/Pedidos.Application/Pedidos.Application.csproj services/Pedidos/Pedidos.Application/
COPY services/Pedidos/Pedidos.Infrastructure/Pedidos.Infrastructure.csproj services/Pedidos/Pedidos.Infrastructure/
COPY services/Pedidos/Pedidos.Api/Pedidos.Api.csproj services/Pedidos/Pedidos.Api/

RUN dotnet restore services/Pedidos/Pedidos.Api/Pedidos.Api.csproj

COPY shared/ shared/
COPY services/Pedidos/ services/Pedidos/

RUN dotnet publish services/Pedidos/Pedidos.Api/Pedidos.Api.csproj \
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
