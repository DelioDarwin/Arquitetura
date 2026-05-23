# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Arquitetura.Shared/Arquitetura.SharedKernel/Arquitetura.SharedKernel.csproj Arquitetura.Shared/Arquitetura.SharedKernel/
COPY Arquitetura.Backend/Clientes/Clientes.Domain/Clientes.Domain/Clientes.Domain.csproj Arquitetura.Backend/Clientes/Clientes.Domain/Clientes.Domain/
COPY Arquitetura.Backend/Clientes/Clientes.Application/Clientes.Application/Clientes.Application.csproj Arquitetura.Backend/Clientes/Clientes.Application/Clientes.Application/
COPY Arquitetura.Backend/Clientes/Clientes.Infrastructure/Clientes.Infrastructure/Clientes.Infrastructure.csproj Arquitetura.Backend/Clientes/Clientes.Infrastructure/Clientes.Infrastructure/
COPY Arquitetura.Backend/Clientes/Clientes.Api/Clientes.Api/Clientes.Api.csproj Arquitetura.Backend/Clientes/Clientes.Api/Clientes.Api/

RUN dotnet restore Arquitetura.Backend/Clientes/Clientes.Api/Clientes.Api/Clientes.Api.csproj

COPY Arquitetura.Shared/ Arquitetura.Shared/
COPY Arquitetura.Backend/Clientes/ Arquitetura.Backend/Clientes/

RUN dotnet publish Arquitetura.Backend/Clientes/Clientes.Api/Clientes.Api/Clientes.Api.csproj \
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

ENTRYPOINT ["dotnet", "Clientes.Api.dll"]

