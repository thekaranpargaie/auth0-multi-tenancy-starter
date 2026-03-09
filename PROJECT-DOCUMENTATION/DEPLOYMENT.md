# Deployment Guide

## Docker Compose (Recommended)

The fastest way to run the full stack:

```bash
cp .env.example .env
# fill in .env

docker compose up --build -d
```

Services:
- Frontend  → http://localhost:5173
- Backend API → http://localhost:8000
- Swagger UI → http://localhost:8000/docs
- Health → http://localhost:8000/health

---

## Production Checklist

- [ ] All Auth0 application URLs updated to production domain
- [ ] HTTPS termination configured (nginx/Traefik/Caddy in front)
- [ ] `FRONTEND_URL` and `VITE_API_BASE_URL` set to production URLs
- [ ] Frontend rebuilt after updating VITE_* variables
- [ ] `ASPNETCORE_ENVIRONMENT=Production` (default in Dockerfile)
- [ ] SMTP configured for email delivery
- [ ] Auth0 M2M credentials rotated and stored as secrets

---

## Update Auth0 URLs for Production

In Auth0 Dashboard, update the SPA application:
- **Allowed Callback URLs**: `https://app.yourdomain.com/callback`
- **Allowed Logout URLs**: `https://app.yourdomain.com`
- **Allowed Web Origins**: `https://app.yourdomain.com`

Update `.env`:
```env
FRONTEND_URL=https://app.yourdomain.com
VITE_API_BASE_URL=https://api.yourdomain.com
```

Rebuild the frontend (VITE_* are baked into the bundle at build time):
```bash
docker compose build frontend
docker compose up -d frontend
```

---

## Rate Limiting (Recommended for Production)

Add to `Program.cs`:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("signup", o =>
    {
        o.Window = TimeSpan.FromMinutes(10);
        o.PermitLimit = 5;
        o.QueueLimit = 0;
    });
});
```

Decorate the controller action:
```csharp
[EnableRateLimiting("signup")]
[HttpPost]
public async Task<IActionResult> Signup(...) { ... }
```

---

## Structured Logging with Serilog

```bash
cd backend/src/Auth0MultiTenancy.API
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
```

```csharp
// Program.cs
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console(new JsonFormatter()));
```

---

## CI/CD — GitHub Actions

```yaml
name: Deploy
on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Build backend
        run: |
          docker build -t myregistry/backend:${{ github.sha }} ./backend
          docker push myregistry/backend:${{ github.sha }}

      - name: Build frontend
        run: |
          docker build \
            --build-arg VITE_AUTH0_DOMAIN=${{ secrets.AUTH0_DOMAIN }} \
            --build-arg VITE_AUTH0_CLIENT_ID=${{ secrets.AUTH0_APP_CLIENT_ID }} \
            --build-arg VITE_AUTH0_AUDIENCE=${{ secrets.AUTH0_AUDIENCE }} \
            --build-arg VITE_API_BASE_URL=${{ secrets.API_URL }} \
            -t myregistry/frontend:${{ github.sha }} ./frontend
          docker push myregistry/frontend:${{ github.sha }}

      - name: Deploy
        run: ssh deploy@server "docker compose pull && docker compose up -d"
```

---

## Horizontal Scaling

The backend is stateless per-instance. For multiple replicas, replace the in-memory
M2M token cache with a distributed Redis cache:

```csharp
// DependencyInjection.cs — replace AddMemoryCache with:
services.AddStackExchangeRedisCache(options =>
    options.Configuration = configuration["Redis:ConnectionString"]);
```

Then update `Auth0TokenCache` to inject `IDistributedCache` instead of `IMemoryCache`.

The frontend is pure static files served by nginx — no configuration needed to scale.
