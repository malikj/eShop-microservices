# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CatalogService/Catalog.Api/Catalog.Api.csproj", "CatalogService/Catalog.Api/"]
COPY ["CatalogService/Catalog.Application/Catalog.Application.csproj", "CatalogService/Catalog.Application/"]
COPY ["CatalogService/Catalog.Domain/Catalog.Domain.csproj", "CatalogService/Catalog.Domain/"]
COPY ["CatalogService/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "CatalogService/Catalog.Infrastructure/"]
COPY ["eShop.Contracts/eShop.Contracts.csproj", "eShop.Contracts/"]

RUN dotnet restore "CatalogService/Catalog.Api/Catalog.Api.csproj"

COPY . .

WORKDIR "/src/CatalogService/Catalog.Api"
RUN dotnet publish -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
