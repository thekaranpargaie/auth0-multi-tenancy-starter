# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first for layer-caching of restore
COPY Auth0MultiTenancy.sln ./
COPY src/Auth0MultiTenancy.Domain/Auth0MultiTenancy.Domain.csproj               src/Auth0MultiTenancy.Domain/
COPY src/Auth0MultiTenancy.Application/Auth0MultiTenancy.Application.csproj     src/Auth0MultiTenancy.Application/
COPY src/Auth0MultiTenancy.Infrastructure/Auth0MultiTenancy.Infrastructure.csproj src/Auth0MultiTenancy.Infrastructure/
COPY src/Auth0MultiTenancy.API/Auth0MultiTenancy.API.csproj                     src/Auth0MultiTenancy.API/

RUN dotnet restore

# Copy all source and publish
COPY . .
RUN dotnet publish src/Auth0MultiTenancy.API/Auth0MultiTenancy.API.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
# Non-root user for security
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8000/health || exit 1

ENTRYPOINT ["dotnet", "Auth0MultiTenancy.API.dll"]
