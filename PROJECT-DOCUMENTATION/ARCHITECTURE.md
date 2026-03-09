# Architecture Guide

## Overview

This application follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture). The key rule is: **dependencies only point inward**. Inner layers know nothing about outer layers.

```
┌──────────────────────────────────────────────────────┐
│  API Layer (ASP.NET Core Controllers, Middleware)    │
│  ┌────────────────────────────────────────────────┐  │
│  │  Infrastructure (Auth0 Client, SMTP Service)  │  │
│  │  ┌────────────────────────────────────────┐   │  │
│  │  │  Application (Use Cases, Interfaces)  │   │  │
│  │  │  ┌────────────────────────────────┐   │   │  │
│  │  │  │  Domain (Entities, Exceptions) │   │   │  │
│  │  │  └────────────────────────────────┘   │   │  │
│  │  └────────────────────────────────────────┘   │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

---

## Layer Details

### Domain Layer (`Auth0MultiTenancy.Domain`)

The innermost ring. Contains:

- **Entities**: `Organization`, `User` — plain C# records with factory methods. No framework dependencies.
- **Domain Exceptions**: Strongly-typed exceptions (`OrganizationAlreadyExistsException`, `UserAlreadyExistsException`, etc.) that represent meaningful business failures.

Rules:
- Zero NuGet dependencies
- No I/O of any kind
- Pure business concepts

### Application Layer (`Auth0MultiTenancy.Application`)

Contains the **use cases** — the "what the system does" without caring "how it does it".

- **Use Cases**: `SignupUseCase`, `InviteUserUseCase` — orchestrate a series of steps using injected interfaces.
- **DTOs**: `SignupRequest`, `SignupResponse`, `InviteUserRequest`, `InviteUserResponse` — data contracts crossing the Application boundary.
- **Interfaces**: `IAuth0ManagementService`, `IEmailService` — contracts that Infrastructure must fulfill.
- **Auth0Settings**: A plain POCO that carries Auth0-related configuration values (populated by Infrastructure during DI wiring).

Rules:
- No dependency on ASP.NET Core, HttpClient, or any external library
- Only depends on Domain and `Microsoft.Extensions.Logging.Abstractions`

### Infrastructure Layer (`Auth0MultiTenancy.Infrastructure`)

Provides concrete implementations of Application interfaces.

- **`Auth0ManagementService`**: Calls the Auth0 Management API via `HttpClient`. Handles token injection, error mapping (HTTP 409 → `UserAlreadyExistsException`), and JSON deserialization.
- **`Auth0TokenCache`**: Fetches M2M access tokens and caches them in `IMemoryCache` with an early-expiry buffer to avoid per-request token round-trips.
- **`SmtpEmailService`**: Sends transactional emails. Gracefully skips sending when SMTP credentials are not configured.
- **`DependencyInjection`**: Extension method `AddInfrastructure()` wires everything together, producing a clean composition root.
- **Options**: `Auth0Options`, `SmtpOptions` — strongly typed configuration sections bound to `appsettings.json` / environment variables.

### API Layer (`Auth0MultiTenancy.API`)

The outermost ring. Responsible for:

- **Controllers**: Thin; extract claims from the JWT, call a use case, return an HTTP response.
- **ExceptionHandlingMiddleware**: Catches domain exceptions and maps them to appropriate HTTP status codes (409, 403, 404, 400, 500) with a unified `{ "detail": "..." }` response shape.
- **Program.cs**: DI composition root — registers services, configures JWT Bearer auth, CORS, Swagger, health checks.
- **Swagger/OpenAPI**: Full JWT bearer documentation via Swashbuckle.

---

## JWT Authentication Flow

```
Browser                        Vue 3 App                    .NET API            Auth0
  │                               │                            │                  │
  │── navigates to /dashboard ──>│                            │                  │
  │                               │── loginWithRedirect ─────────────────────>  │
  │                               │                            │                  │
  │<─ Auth0 redirect ────────────────────────────────────────────────────────── │
  │── /callback?code=... ────────>│                            │                  │
  │                               │── handleRedirectCallback ──────────────────> │
  │                               │<─ access_token (JWT) ──────────────────────  │
  │                               │                            │                  │
  │                               │── POST /api/invite ──────>│                  │
  │                               │   Authorization: Bearer    │                  │
  │                               │   <access_token>           │── JWKS fetch ──> │
  │                               │                            │<─ public keys ── │
  │                               │                            │── validate JWT   │
  │                               │<─ 201 Created ────────────│                  │
```

### JWT Claims Used

| Claim | Source | Used for |
|---|---|---|
| `sub` | Auth0 | Caller identity in logs |
| `org_id` | Auth0 Organizations | Org isolation checks |
| `org_display_name` | Auth0 Organizations | Display in frontend |
| `exp` | Auth0 | Token expiry validation |

---

## M2M Token Cache Design

The `Auth0TokenCache` singleton prevents a new token fetch on every API request:

```
Request arrives
     │
     ▼
IMemoryCache.TryGetValue("auth0_m2m_token")
     │
 hit ├──> return cached token (< 1ms)
     │
miss └──> POST /oauth/token (200-500ms)
           │
           ▼
         Store in cache for (expires_in - 60) seconds
           │
           ▼
         Return token
```

The 60-second buffer ensures the token is never used within one minute of its real expiry, preventing race conditions in high-throughput scenarios.

---

## Frontend Architecture

The Vue 3 frontend follows a layered approach:

```
Router guards
    │
    ▼
Pinia authStore (single source of truth for Auth0 state)
    │
    ├── Pages (Signup, Login, Dashboard, UserManagement)
    │       │
    │       └── apiService.js (Axios + request interceptor attaches JWT)
    │
    └── App.vue (global navigation bar, transitions)
```

### State Management (Pinia `authStore`)

| State | Type | Description |
|---|---|---|
| `client` | `Auth0Client` | Auth0 SPA SDK instance |
| `isAuthenticated` | `boolean` | Auth state |
| `user` | `object` | Auth0 user profile |
| `orgId` | `string` | `org_id` from ID token claims |
| `orgDisplayName` | `string` | Organization display name |
| `loading` | `boolean` | True while `init()` runs |

### Request Interceptor

Every Axios request automatically receives the JWT:

```js
apiClient.interceptors.request.use(async (config) => {
  const token = await authStore.getToken()  // silent refresh if needed
  config.headers.Authorization = `Bearer ${token}`
  return config
})
```

Public endpoints (signup, health) pass `{ _skipAuth: true }` to bypass this.

---

## Dependency Graph

```
Auth0MultiTenancy.API
  ├── Auth0MultiTenancy.Application
  │     └── Auth0MultiTenancy.Domain
  └── Auth0MultiTenancy.Infrastructure
        └── Auth0MultiTenancy.Application
              └── Auth0MultiTenancy.Domain
```

`Domain` has no dependencies.  
`Application` depends only on `Domain`.  
`Infrastructure` depends on `Application` (to implement its interfaces).  
`API` depends on `Application` (to call use cases) and `Infrastructure` (to register its services).
