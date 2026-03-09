# Environment Variables Documentation

## Overview

This project uses environment variables for configuration. There are two main locations where environment variables are defined:

1. **Root `.env` file** - Used for frontend build-time configuration
2. **Backend `.env` file** - Used for backend runtime configuration

## Root .env File (Frontend Build Variables)

Located at project root. These variables are embedded into the frontend application during Docker build.

### Required Variables

| Variable | Description | Example | Purpose |
|----------|-------------|---------|---------|
| `VITE_AUTH0_DOMAIN` | Your Auth0 tenant domain | `acme.us.auth0.com` | Auth0 SPA application domain |
| `VITE_AUTH0_CLIENT_ID` | SPA Application Client ID | `AAmzgxmca21tfjxDJdEebuZ0rLwbupCu` | Client ID for frontend Auth0 integration |
| `VITE_AUTH0_AUDIENCE` | Auth0 Management API audience | `https://acme.us.auth0.com/api/v2/` | API identifier for Auth0 Management API |
| `VITE_API_BASE_URL` | Backend API URL from browser | `http://localhost:8000` | URL where frontend can reach backend |

### How It Works
- These variables are prefixed with `VITE_` to be exposed by Vite during build
- They become available as `import.meta.env.VITE_*` in frontend code
- Changes require rebuilding the frontend container
- Used in `frontend/src/config/auth.js`

## Backend .env File (Runtime Variables)

Located at `backend/.env`. These variables are loaded at runtime by the Python backend.

### Auth0 Configuration (Required)

| Variable | Required | Example | Description |
|----------|----------|---------|-------------|
| `AUTH0_DOMAIN` | ✅ | `acme.us.auth0.com` | Auth0 tenant domain |
| `AUTH0_M2M_CLIENT_ID` | ✅ | `kZbjpfOsSoPp0rzzEX9HN7iN9Y9XUyn6` | Machine-to-Machine Application Client ID |
| `AUTH0_M2M_CLIENT_SECRET` | ✅ | `xEHVKosYZxNmjYTetOnxCvVI8zMLP7VJ...` | M2M Application Client Secret (secret!) |
| `AUTH0_APP_CLIENT_ID` | ✅ | `AAmzgxmca21tfjxDJdEebuZ0rLwbupCu` | SPA Application Client ID (for organization signup) |
| `AUTH0_AUDIENCE` | ✅ | `https://acme.us.auth0.com/api/v2/` | Auth0 Management API audience |
| `AUTH0_ADMIN_ROLE_ID` | ✅ | `rol_CfCLnlOA8pCGEo3T` | Admin role ID from Auth0 Organizations |
| `AUTH0_MEMBER_ROLE_ID` | ✅ | `rol_Vf562LXvVejQCnFN` | Member role ID from Auth0 Organizations |
| `AUTH0_CONNECTION` | ❌ | `Username-Password-Authentication` | Auth0 connection name (default: Username-Password-Authentication) |

### Application Configuration (Required)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `FRONTEND_URL` | ✅ | — | Frontend URL for CORS and redirects |
| `HOST` | ✅ | `0.0.0.0` | API server host (keep for Docker) |
| `PORT` | ✅ | `8000` | API server port |
| `DEBUG` | ✅ | `false` | Enable debug mode |

### Email Configuration (Optional)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `SMTP_SERVER` | ❌ | `localhost` | SMTP server hostname |
| `SMTP_PORT` | ❌ | `587` | SMTP server port |
| `SMTP_USERNAME` | ❌ | — | SMTP username/email |
| `SMTP_PASSWORD` | ❌ | — | SMTP password (use app-specific for Gmail) |
| `SMTP_FROM_EMAIL` | ❌ | `noreply@example.com` | Email sender address |
| `SMTP_USE_TLS` | ❌ | `true` | Use TLS encryption |

## Environment Setup Process

### 1. Copy Example Files
```bash
# Root .env
cp .env.example .env

# Backend .env
cp backend/.env.example backend/.env
```

### 2. Get Auth0 Credentials

From Auth0 Dashboard:
1. **SPA Application** (for frontend)
   - Applications > Applications > Create Application (Single Page)
   - Note: Client ID, Domain

2. **M2M Application** (for backend)
   - Applications > Applications > Create Application (Machine to Machine)
   - Select "Auth0 Management API"
   - Add permissions: `create:users`, `read:users`, `update:users`, `create:organizations`, `read:organizations`, `create:organization_members`, `create:organization_invitations`
   - Note: Client ID, Client Secret

3. **Organization Roles**
   - Organizations > Roles
   - Note: Admin and Member Role IDs (format: `rol_xxx`)

### 3. Fill Environment Variables

#### Root .env
```env
VITE_AUTH0_DOMAIN=your-tenant.auth0.com
VITE_AUTH0_CLIENT_ID=your_spa_client_id
VITE_AUTH0_AUDIENCE=https://your-tenant.auth0.com/api/v2/
VITE_API_BASE_URL=http://localhost:8000
```

#### Backend .env
```env
# Auth0
AUTH0_DOMAIN=your-tenant.auth0.com
AUTH0_M2M_CLIENT_ID=your_m2m_client_id
AUTH0_M2M_CLIENT_SECRET=your_m2m_client_secret
AUTH0_APP_CLIENT_ID=your_spa_client_id
AUTH0_AUDIENCE=https://your-tenant.auth0.com/api/v2/
AUTH0_ADMIN_ROLE_ID=rol_xxx
AUTH0_MEMBER_ROLE_ID=rol_yyy

# App
FRONTEND_URL=http://localhost:5173
HOST=0.0.0.0
PORT=8000
DEBUG=true

# Email (optional)
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM_EMAIL=noreply@yourcompany.com
```

### 4. Validate Configuration

Test configuration:
```bash
# Backend
cd backend
python -c "from config import settings; print('Config loaded successfully')"

# Frontend (build test)
docker-compose build frontend
```

## Important Notes

1. **Security**: Never commit `.env` files to version control
2. **Build vs Runtime**: Root `.env` values are baked into the frontend at build time
3. **CORS**: `FRONTEND_URL` must match your frontend domain exactly
4. **Debug Mode**: Set `DEBUG=true` in development, `false` in production
5. **Email**: SMTP configuration is optional - app works without emails but won't send notifications
6. **Auth0 Connection**: Usually `Username-Password-Authentication` - check your Auth0 dashboard

## Troubleshooting Common Issues

1. **Frontend not picking up variables**
   - Rebuild frontend after changing root `.env`
   - Verify variable names start with `VITE_`

2. **Backend authentication errors**
   - Check `AUTH0_M2M_CLIENT_ID` and `AUTH0_M2M_CLIENT_SECRET`
   - Verify M2M application has all required permissions

3. **CORS errors**
   - Ensure `FRONTEND_URL` matches frontend domain
   - Check for trailing slashes in URLs

4. **Role assignment errors**
   - Verify `AUTH0_ADMIN_ROLE_ID` and `AUTH0_MEMBER_ROLE_ID` are correct
   - Check role IDs are from your Auth0 Organizations section