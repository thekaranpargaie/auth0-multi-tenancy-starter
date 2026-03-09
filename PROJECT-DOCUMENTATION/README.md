# Auth0 Multi-Tenant SaaS

> **Stack**: .NET 10 (ASP.NET Core) · Vue 3 + Vite · Auth0 Organizations · Docker Compose

A production-ready, multi-tenant SaaS scaffold built with Clean Architecture (SOLID), JWT authentication, role-based access control, and Docker-first deployment.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [Quick Start (Docker)](#quick-start-docker)
4. [Local Development (no Docker)](#local-development-no-docker)
5. [Auth0 Setup](#auth0-setup)
6. [Environment Variables](#environment-variables)
7. [API Reference](#api-reference)
8. [Frontend Routes](#frontend-routes)
9. [Security](#security)
10. [Troubleshooting](#troubleshooting)
11. [Architecture Decision Records](#architecture-decision-records)

---

## Architecture Overview

```
┌──────────────────────────┐   HTTPS/JWT    ┌───────────────────────────────────────────┐
│  Vue 3 SPA (nginx:80)   │ ─────────────> │  ASP.NET Core 10 API (:8000)              │
│                          │               │                                           │
│  • Auth0 SPA SDK         │               │  API Layer (Controllers, Middleware)      │
│  • Pinia store           │               │       │                                   │
│  • Vue Router guards     │               │  Application Layer (Use Cases, DTOs)      │
│  • Axios + auto-JWT      │               │       │                                   │
└──────────────────────────┘               │  Infrastructure (Auth0 Client, SMTP)      │
                                           │       │                                   │
                                           │  Domain (Entities, Exceptions)            │
                                           └───────────────────────┬───────────────────┘
                                                                   │ M2M Token
                                                      ┌────────────▼──────────────┐
                                                      │   Auth0 Management API    │
                                                      │  Orgs · Users · Roles     │
                                                      └───────────────────────────┘
```

### Clean Architecture Layers

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `Auth0MultiTenancy.Domain` | Entities, domain exceptions. Zero external dependencies. |
| **Application** | `Auth0MultiTenancy.Application` | Use-case orchestrators, DTOs, service interfaces. Depends only on Domain. |
| **Infrastructure** | `Auth0MultiTenancy.Infrastructure` | Auth0 Management API client (with M2M token cache), SMTP email. |
| **API** | `Auth0MultiTenancy.API` | ASP.NET Core 10 controllers, JWT middleware, Swagger, DI root. |

### SOLID Principles Applied

- **S** — Each class has one reason to change (`Auth0ManagementService` handles HTTP; `Auth0TokenCache` handles caching; `SignupUseCase` handles the signup flow).
- **O** — Adding new auth providers or email providers only requires a new implementation; existing code is untouched.
- **L** — All service implementations satisfy their interface contracts without surprises.
- **I** — Separate `IAuth0ManagementService` and `IEmailService` rather than one fat interface.
- **D** — Controllers and use-cases depend on abstractions (interfaces), never on concrete classes.

---

## Project Structure

```
auth0-multi-tenancy/
├── .env.example                           ← Copy to .env and fill in values
├── .gitignore
├── docker-compose.yml                     ← Single command startup
│
├── backend/                               ← .NET 10 solution
│   ├── Auth0MultiTenancy.sln
│   ├── Dockerfile
│   ├── .env.example
│   └── src/
│       ├── Auth0MultiTenancy.Domain/
│       │   ├── Entities/Organization.cs
│       │   ├── Entities/User.cs
│       │   └── Exceptions/DomainExceptions.cs
│       │
│       ├── Auth0MultiTenancy.Application/
│       │   ├── DTOs/SignupDto.cs
│       │   ├── DTOs/InviteUserDto.cs
│       │   ├── Interfaces/IAuth0ManagementService.cs
│       │   ├── Interfaces/IEmailService.cs
│       │   ├── UseCases/Auth0Settings.cs
│       │   ├── UseCases/SignupUseCase.cs
│       │   └── UseCases/InviteUserUseCase.cs
│       │
│       ├── Auth0MultiTenancy.Infrastructure/
│       │   ├── Auth0/Auth0ManagementService.cs
│       │   ├── Auth0/Auth0TokenCache.cs
│       │   ├── Configuration/InfrastructureOptions.cs
│       │   ├── Email/SmtpEmailService.cs
│       │   └── DependencyInjection.cs
│       │
│       └── Auth0MultiTenancy.API/
│           ├── Controllers/SignupController.cs
│           ├── Controllers/InviteController.cs
│           ├── Controllers/OrganizationsController.cs
│           ├── Middleware/ExceptionHandlingMiddleware.cs
│           ├── Program.cs
│           └── appsettings.json
│
├── frontend/                              ← Vue 3 + Vite SPA
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── package.json
│   ├── vite.config.js
│   ├── index.html
│   ├── .env.example
│   └── src/
│       ├── config/auth.js
│       ├── stores/authStore.js            ← Pinia auth store
│       ├── services/apiService.js         ← Axios + auto JWT
│       ├── router/index.js
│       ├── pages/Signup.vue
│       ├── pages/Login.vue
│       ├── pages/Callback.vue
│       ├── pages/Dashboard.vue
│       ├── pages/UserManagement.vue
│       ├── App.vue
│       └── main.js
│
└── PROJECT-DOCUMENTATION/
    ├── README.md                          ← This file
    ├── API-DOCUMENTATION.md
    ├── AUTH0-CONFIGURATION.md
    ├── ENVIRONMENT-VARIABLES.md
    ├── PERMISSIONS-ROLES.md
    ├── ARCHITECTURE.md
    ├── WORKFLOWS.md
    ├── SECURITY.md
    └── DEPLOYMENT.md
```

---

## Quick Start (Docker)

### Prerequisites
- Docker Desktop (or Docker Engine + Compose v2)
- Auth0 account with Organizations enabled
- ~5 minutes for Auth0 configuration (see [Auth0 Setup](#auth0-setup))

### Steps

```bash
# 1. Navigate to the project
cd auth0-multi-tenancy

# 2. Create your environment file
cp .env.example .env

# 3. Edit .env — fill in all AUTH0_* values (see Auth0 Setup below)

# 4. Build and start all services
docker compose up --build -d

# 5. Check health
docker compose ps
docker compose logs -f
```

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| Backend API | http://localhost:8000 |
| Swagger UI | http://localhost:8000/docs |
| Health Check | http://localhost:8000/health |

```bash
# Stop
docker compose down

# Stop + remove volumes
docker compose down -v

# Rebuild one service after a code change
docker compose build backend && docker compose up -d backend
docker compose build frontend && docker compose up -d frontend
```

---

## Local Development (no Docker)

### Backend (.NET 10)

```bash
cd backend

# Restore and build
dotnet restore
dotnet build

# Set env vars or use .NET User Secrets:
cd src/Auth0MultiTenancy.API
dotnet user-secrets set "Auth0:Domain"             "your-tenant.us.auth0.com"
dotnet user-secrets set "Auth0:Audience"           "https://your-tenant.us.auth0.com/api/v2/"
dotnet user-secrets set "Auth0:M2MClientId"        "..."
dotnet user-secrets set "Auth0:M2MClientSecret"    "..."
dotnet user-secrets set "Auth0:AppClientId"        "..."
dotnet user-secrets set "Auth0:AdminRoleId"        "rol_..."
dotnet user-secrets set "Auth0:MemberRoleId"       "rol_..."
dotnet user-secrets set "App:FrontendUrl"          "http://localhost:5173"

# Run with live reload
dotnet watch run --project src/Auth0MultiTenancy.API
# API: http://localhost:8000  |  Swagger: http://localhost:8000/docs
```

### Frontend (Vue 3)

```bash
cd frontend

cp .env.example .env
# Edit .env — fill in VITE_AUTH0_DOMAIN, VITE_AUTH0_CLIENT_ID, VITE_AUTH0_AUDIENCE

npm install
npm run dev        # http://localhost:5173 with hot reload
npm run build      # Production build to dist/
```

---

## Auth0 Setup

### Step 1 — Enable Organizations
1. Auth0 Dashboard → **Settings** → **Organizations** → Enable → Save

### Step 2 — Create SPA Application (frontend)
1. **Applications** → **Create Application** → **Single Page Web Application**
2. Name: `Multi-Tenant SPA`
3. **Allowed Callback URLs**: `http://localhost:5173/callback`
4. **Allowed Logout URLs**: `http://localhost:5173`
5. **Allowed Web Origins**: `http://localhost:5173`
6. Copy **Client ID** → `AUTH0_APP_CLIENT_ID`
7. Copy **Domain** → `AUTH0_DOMAIN`

### Step 3 — Create M2M Application (backend)
1. **Applications** → **Create Application** → **Machine to Machine**
2. Name: `Multi-Tenant Backend M2M`
3. Select **Auth0 Management API**
4. Grant these permission scopes:

| Scope | Used for |
|---|---|
| `create:users` | Signup / Invite |
| `read:users` | Read user info |
| `update:users` | Update users |
| `create:organizations` | Create org on signup |
| `read:organizations` | Read org details |
| `create:organization_members` | Add member to org |
| `create:organization_invitations` | Invite flow |
| `create:user_tickets` | Password-change link |

5. Copy **Client ID** → `AUTH0_M2M_CLIENT_ID`
6. Copy **Client Secret** → `AUTH0_M2M_CLIENT_SECRET`

### Step 4 — Create Organization Roles
1. **Organizations** → **Roles** → **Create Role**
2. Create **Admin** → copy ID → `AUTH0_ADMIN_ROLE_ID`
3. Create **Member** → copy ID → `AUTH0_MEMBER_ROLE_ID`
> Role IDs are in the format `rol_xxxxxxxxxxxxxxxx`

---

## Environment Variables

A single `.env` at the project root (copy from `.env.example`) feeds both services.

### Required

| Variable | Description | Example |
|---|---|---|
| `AUTH0_DOMAIN` | Auth0 tenant domain | `acme.us.auth0.com` |
| `AUTH0_AUDIENCE` | Management API audience | `https://acme.us.auth0.com/api/v2/` |
| `AUTH0_APP_CLIENT_ID` | SPA Client ID | `AAmzgx...` |
| `AUTH0_M2M_CLIENT_ID` | M2M Client ID | `kZbjpf...` |
| `AUTH0_M2M_CLIENT_SECRET` | M2M Client Secret | `xEHVKo...` |
| `AUTH0_ADMIN_ROLE_ID` | Admin role ID | `rol_CfCL...` |
| `AUTH0_MEMBER_ROLE_ID` | Member role ID | `rol_Vf56...` |

### Optional

| Variable | Default | Description |
|---|---|---|
| `AUTH0_CONNECTION` | `Username-Password-Authentication` | Auth0 DB connection |
| `FRONTEND_URL` | `http://localhost:5173` | CORS allowed origin |
| `VITE_API_BASE_URL` | `http://localhost:8000` | Backend URL visible to browser |
| `SMTP_SERVER` | `localhost` | SMTP hostname |
| `SMTP_PORT` | `587` | SMTP port |
| `SMTP_USERNAME` | _(empty)_ | If empty, emails are skipped |
| `SMTP_PASSWORD` | _(empty)_ | SMTP password |
| `SMTP_FROM_EMAIL` | `noreply@example.com` | Sender address |
| `SMTP_USE_TLS` | `true` | Enable STARTTLS |

---

## API Reference

Base URL: `http://localhost:8000`
Interactive docs: `http://localhost:8000/docs`

### `GET /health`
Health check — no authentication required.
**Response 200**: `{ "status": "Healthy" }`

---

### `POST /api/signup` — Public
Creates a new organization and its first admin user.

**Body**:
```json
{
  "name": "Jane Doe",
  "email": "jane@acme.com",
  "organizationName": "Acme Corp",
  "organizationDomain": "acme.com"
}
```

**201 Created**: `{ "message": "Organization and user created successfully..." }`
**400**: Validation error | **409**: Org or user already exists

---

### `POST /api/invite` — Authenticated
Invites a user to the caller's organization. Requires JWT with `org_id` claim.

**Body**:
```json
{
  "organizationId": "org_abc123",
  "email": "newuser@acme.com",
  "role": 0
}
```
> `role`: `0` = Member, `1` = Admin

**201 Created**: `{ "message": "User ... invited successfully." }`
**401** | **403** | **409**

---

### `GET /api/organizations/{organizationId}/members` — Authenticated
Returns all members of the specified organization (caller must be a member).

**200 OK**:
```json
[{ "userId": "auth0|abc", "email": "jane@acme.com", "name": "Jane", "emailVerified": true }]
```

---

### `DELETE /api/organizations/{organizationId}/members/{userId}` — Authenticated
Removes a user from the organization.
**204 No Content**

---

## Frontend Routes

| Path | Page | Auth Required |
|---|---|---|
| `/signup` | Signup.vue | No |
| `/login` | Login.vue | No |
| `/callback` | Callback.vue | No |
| `/dashboard` | Dashboard.vue | **Yes** |
| `/users` | UserManagement.vue | **Yes** |

---

## Security

| Concern | Implementation |
|---|---|
| JWT Verification | RS256 via Auth0 JWKS; issuer + audience + expiry validated on every request |
| CORS | Only `FRONTEND_URL` allowed; credentials enabled |
| Org Isolation | Every protected endpoint validates caller `org_id` === target `organizationId` |
| M2M Credentials | Backend-only; never sent to browser |
| Token Caching | M2M token cached in `IMemoryCache` with 60-second early-expiry buffer |
| Input Validation | Data-annotation validators on all request DTOs before use-cases execute |
| Error Responses | Global middleware returns `{ "detail": "..." }` — no stack traces in production |
| Security Headers | nginx: `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`, `Referrer-Policy` |

---

## Troubleshooting

**Backend container won't start**
```bash
docker compose logs backend
# Look for missing Auth0:* configuration values
```

**Frontend shows blank page / Auth0 errors**
- Any change to `.env` requires a rebuild: `docker compose build frontend && docker compose up -d frontend`
- VITE_* variables are baked into the bundle at build time — runtime changes have no effect.

**`401 Unauthorized` on API calls**
- Confirm `AUTH0_AUDIENCE` exactly matches the audience your SPA requests.
- `AUTH0_DOMAIN` must have no trailing slash.

**Role assignment fails (500)**
- Role IDs must come from **Organizations → Roles**, not **User Management → Roles** (they are different).

**Email not sending**
- The app works without SMTP. Email is optional.
- Set `SMTP_USERNAME` in `.env` to enable. Check `docker compose logs backend` for SMTP errors.

**CORS errors in browser**
- `FRONTEND_URL` must exactly match the browser origin: scheme + host + port, no trailing slash.

---

## Architecture Decision Records

### ADR-001: Clean Architecture
Using Clean Architecture keeps Auth0 as an infrastructure concern. The Application layer never references `HttpClient` or Auth0 JSON schemas — only contracts (`IAuth0ManagementService`). This makes tests trivial: inject a mock and test use-case logic in isolation.

### ADR-002: Pinia over Vuex
Pinia is the officially recommended Vue store manager with first-class TypeScript support, Vue DevTools integration, and a significantly simpler action model.

### ADR-003: nginx for the frontend
nginx handles SPA fallback routing, gzip, aggressive caching headers, and security headers — all production requirements that Vite's preview server doesn't address.

### ADR-004: In-memory M2M token cache
Auth0 M2M token fetches add ~200–500 ms. Tokens are valid for 24 hours. Caching in `IMemoryCache` with a 60-second safety buffer eliminates per-request latency with negligible memory cost.

### ADR-005: Single root `.env`
One file, one `docker compose up`. Eliminates the cognitive overhead of managing separate `backend/.env` and `frontend/.env` during local development and onboarding.
