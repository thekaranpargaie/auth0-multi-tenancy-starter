# API Documentation

## Overview

The API is an ASP.NET Core 10 Web API. It provides endpoints for organization signup, user invitation, and member management. Authentication uses RS256 JWT tokens issued by Auth0.

## Base URL

| Environment | URL |
|---|---|
| Development | `http://localhost:8000` |
| Swagger UI | `http://localhost:8000/docs` |

## Authentication

All protected endpoints require a JWT token in the `Authorization` header:
```
Authorization: Bearer <access_token>
```

Tokens are verified against Auth0 JWKS. Required claims:
- `sub` — User identifier
- `org_id` — Organization context (for protected endpoints)
- `exp` — Expiration time

## Endpoints

---

### `GET /health`

Health check. No authentication required.

**Response 200**:
```json
{ "status": "Healthy" }
```

---

### `POST /api/signup`

Creates a new Auth0 organization and its first admin user.

**Authentication**: None (public endpoint)

**Request Body**:
```json
{
  "name": "Jane Doe",
  "email": "jane@acme.com",
  "organizationName": "Acme Corp",
  "organizationDomain": "acme.com"
}
```

| Field | Required | Rules |
|---|---|---|
| `name` | Yes | 2–100 characters |
| `email` | Yes | Valid email address |
| `organizationName` | Yes | 2–100 characters |
| `organizationDomain` | No | Max 100 characters |

**Response 201 Created**:
```json
{
  "message": "Organization and user created successfully. Please check your email to set your password."
}
```

**Workflow**:
1. Derives an org slug from `organizationDomain` (or `organizationName`)
2. Creates the organization in Auth0
3. Creates the user in Auth0 with a temporary password
4. Adds user to org as Admin
5. Sends password-change ticket via Auth0
6. Optionally sends welcome email (if SMTP configured)

**Error Responses**:
- `400 Bad Request` — Validation failed
- `409 Conflict` — Organization or user already exists

---

### `POST /api/invite`

Invites a user to an existing organization.

**Authentication**: Required — `org_id` claim must match `organizationId` in the body.

**Request Body**:
```json
{
  "organizationId": "org_abc123",
  "email": "newuser@acme.com",
  "role": 0
}
```

| Field | Required | Values |
|---|---|---|
| `organizationId` | Yes | Auth0 org ID (`org_xxx`) |
| `email` | Yes | Valid email |
| `role` | No | `0` = Member (default), `1` = Admin |

**Response 201 Created**:
```json
{
  "message": "User newuser@acme.com invited successfully. Password reset link sent to their email."
}
```

**Error Responses**:
- `401 Unauthorized` — Missing/invalid token
- `403 Forbidden` — Caller not a member of the target org
- `409 Conflict` — User already exists

---

### `GET /api/organizations/{organizationId}/members`

Returns all members of an organization. Caller must belong to that organization.

**Authentication**: Required

**Path Parameters**:
- `organizationId` — Auth0 organization ID

**Response 200 OK**:
```json
[
  {
    "userId": "auth0|abc123",
    "email": "jane@acme.com",
    "name": "Jane Doe",
    "emailVerified": true
  }
]
```

**Error Responses**:
- `401 Unauthorized`
- `403 Forbidden` — Caller not in that org

---

### `DELETE /api/organizations/{organizationId}/members/{userId}`

Removes a user from an organization. Caller must belong to that organization.

**Authentication**: Required

**Path Parameters**:
- `organizationId` — Auth0 organization ID
- `userId` — Auth0 user ID (e.g. `auth0|abc123`)

**Response**: `204 No Content`

**Error Responses**:
- `401 Unauthorized`
- `403 Forbidden`

---

## Standard Error Response Shape

All errors return:
```json
{
  "detail": "Human-readable error message"
}
```

| HTTP Code | Scenario |
|---|---|
| `400` | Invalid request data |
| `401` | Missing or invalid JWT |
| `403` | Organization access denied |
| `404` | Resource not found |
| `409` | Organization or user conflict |
| `500` | Unexpected server error |

---

## CORS

The API only accepts cross-origin requests from `FRONTEND_URL` (configured via environment variable). In development this is `http://localhost:5173`.

## Rate Limiting

Not currently implemented. Recommended additions for production:
- Signup: max 5 requests per IP per 10 minutes
- Invite: max 20 requests per org per hour
