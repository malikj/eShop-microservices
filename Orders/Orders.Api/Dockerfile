# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (FIXED PATHS)
COPY ["Orders/Orders.Api/Orders.Api.csproj", "Orders/Orders.Api/"]
COPY ["Orders/Orders.Application/Orders.Application.csproj", "Orders/Orders.Application/"]
COPY ["Orders/Orders.Domain/Orders.Domain.csproj", "Orders/Orders.Domain/"]
COPY ["Orders/Orders.Infrastructure/Orders.Infrastructure.csproj", "Orders/Orders.Infrastructure/"]
COPY ["eShop.Contracts/eShop.Contracts.csproj", "eShop.Contracts/"]

# Restore dependencies
RUN dotnet restore "Orders/Orders.Api/Orders.Api.csproj"

# Copy everything else
COPY . .

# Build & publish
WORKDIR "/src/Orders/Orders.Api"
RUN dotnet publish -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Orders.Api.dll"]
