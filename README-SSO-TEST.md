# Auth0 SSO Test with Two Frontend Applications

This setup demonstrates Auth0 Single Sign-On (SSO) across two separate frontend applications: CRM and Notes.

## Architecture

```
┌─────────────────┐     Auth0 SSO     ┌─────────────────┐
│   CRM App       │ <──────────────> │   Auth0        │
│ (Vue 3)         │                 │ (Identity       │
│ localhost:5174   │                 │ Provider)       │
│                 │ JWT Exchange     │                 │
│                 │                 │                 │
│                 │                 │                 │
└─────────────────┘                 └─────────────────┘
      │                               │
      ▼                               ▼
┌─────────────────┐                 ┌─────────────────┐
│   Notes App     │                 │   CRM App       │
│ (React)         │ <──────────────> │ (Vue 3)         │
│ localhost:5175   │   JWT Exchange   │ localhost:5174   │
│                 │                 │                 │
│                 │                 │                 │
└─────────────────┘                 └─────────────────┘
```

## Setup Instructions

### 1. Configure Auth0 Applications

Create two separate Auth0 Single Page Applications:

#### CRM Application
- Name: `crm-client`
- Allowed Callback URLs: `http://localhost:5174/callback`
- Allowed Logout URLs: `http://localhost:5174/`
- Allowed Web Origins: `http://localhost:5174`
- Note the Client ID

#### Notes Application
- Name: `notes-client`
- Allowed Callback URLs: `http://localhost:5175/callback`
- Allowed Logout URLs: `http://localhost:5175/`
- Allowed Web Origins: `http://localhost:5175`
- Note the Client ID

### 2. Update Environment Variables

Create `.env` file:

```env
# Auth0 Configuration
AUTH0_DOMAIN=your-tenant.us.auth0.com
VITE_CRM_CLIENT_ID=crm_client_id_here
VITE_NOTES_CLIENT_ID=notes_client_id_here
VITE_AUTH0_AUDIENCE=https://your-tenant.us.auth0.com/api/v2/

# Optional: Backend API URL (not used in this demo)
VITE_API_BASE_URL=http://localhost:8000
```

### 3. Build and Run Applications

```bash
# Build CRM App
docker-compose build crm-app

# Build Notes App
docker-compose build notes-app

# Start both applications
docker-compose up -d crm-app notes-app

# Check logs
docker-compose logs -f crm-app notes-app
```

## Testing SSO Flow

### Step 1: Login to CRM App
1. Open browser tab: `http://localhost:5174`
2. Click through authentication process with Auth0
3. After successful login, dashboard shows:
   - User information
   - JWT token details (decoded)
   - Access token

### Step 2: Test SSO with Notes App
1. Open new browser tab: `http://localhost:5175`
2. Should see that you're already logged in
3. No need to re-authenticate
4. Dashboard shows your profile information

### Step 3: Verify Tokens
- Both applications should show the same user ID in their JWTs
- Organization claims should match
- Session should be shared across applications

### Step 4: Test Logout
1. Logout from CRM App (port 5174)
2. Try to access Notes App (port 5175)
3. Should be logged out and redirected to Auth0 login

## Key Observations

1. **Same Auth0 Tenant**: Both applications use the same Auth0 tenant
2. **Different Client IDs**: Each application has its own client ID
3. **Session Sharing**: Auth0 manages the session across applications
4. **JWT Validation**: Each app can validate the JWT independently
5. **Organization Context**: JWT contains org_id and organization claims

## Technical Details

### CRM App (Vue 3)
- Framework: Vue 3 + Vite
- Auth Library: @auth0/auth0-spa-js
- Port: 5174
- Component: authStore.js handles all auth logic

### Notes App (React)
- Framework: React + Vite
- Auth Library: @auth0/auth0-react
- Port: 5175
- Component: AuthProvider wraps the entire app

### JWT Structure
Both applications receive JWTs with:
```
{
  "iss": "https://your-tenant.us.auth0.com/",
  "sub": "auth0|12345",
  "aud": "https://your-tenant.us.auth0.com/api/v2/",
  "org_id": "org_abc123",
  "org_display_name": "Acme Corp",
  "roles": ["Admin"],
  "exp": 1741923456
}
```

## Troubleshooting

### Port Conflicts
- Default CRM port: 5174
- Default Notes port: 5175
- Change ports in docker-compose.yml if needed

### Auth0 Configuration
- Ensure callback URLs match exactly
- Both applications must use the same Auth0 tenant
- Domain must not have trailing slash

### JWT Issues
- Check audience matches across both apps
- Verify token expiration time
- Ensure org_id claims are present

### Docker Issues
- Run `docker-compose down` to reset
- Rebuild with `--build` flag
- Check logs for error messages

## Next Steps

This PoC demonstrates the basic SSO flow. Next steps would be:

1. Add more applications (Ticket, etc.)
2. Implement backend API validation
3. Add organization isolation
4. Customize login page
5. Add advanced features (MFA, etc.)