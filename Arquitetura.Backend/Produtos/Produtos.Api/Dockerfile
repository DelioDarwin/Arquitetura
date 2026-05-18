# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto primeiro para aproveitar o cache de layers do Docker
COPY Arquitetura.Shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj Arquitetura.Shared/Arquitetura.SharedKernel/
COPY Arquitetura.Backend/Produtos/Produtos.Domain/Produtos.Domain.csproj Arquitetura.Backend/Produtos/Produtos.Domain/
COPY Arquitetura.Backend/Produtos/Produtos.Application/Produtos.Application.csproj Arquitetura.Backend/Produtos/Produtos.Application/
COPY Arquitetura.Backend/Produtos/Produtos.Infrastructure/Produtos.Infrastructure.csproj Arquitetura.Backend/Produtos/Produtos.Infrastructure/
COPY Arquitetura.Backend/Produtos/Produtos.Api/Produtos.Api.csproj Arquitetura.Backend/Produtos/Produtos.Api/

RUN dotnet restore Arquitetura.Backend/Produtos/Produtos.Api/Produtos.Api.csproj

# Copia o restante do código
COPY Arquitetura.Shared/ Arquitetura.Shared/
COPY Arquitetura.Backend/Produtos/ Arquitetura.Backend/Produtos/

RUN dotnet publish Arquitetura.Backend/Produtos/Produtos.Api/Produtos.Api.csproj \
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

