# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto primeiro para aproveitar o cache de layers do Docker
COPY shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj shared/Arquitetura.SharedKernel/
COPY services/Produtos/Produtos.Domain/Produtos.Domain.csproj services/Produtos/Produtos.Domain/
COPY services/Produtos/Produtos.Application/Produtos.Application.csproj services/Produtos/Produtos.Application/
COPY services/Produtos/Produtos.Infrastructure/Produtos.Infrastructure.csproj services/Produtos/Produtos.Infrastructure/
COPY services/Produtos/Produtos.Api/Produtos.Api.csproj services/Produtos/Produtos.Api/

RUN dotnet restore services/Produtos/Produtos.Api/Produtos.Api.csproj

# Copia o restante do código
COPY shared/ shared/
COPY services/Produtos/ services/Produtos/

RUN dotnet publish services/Produtos/Produtos.Api/Produtos.Api.csproj \
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

ENTRYPOINT ["dotnet", "Produtos.Api.dll"]
