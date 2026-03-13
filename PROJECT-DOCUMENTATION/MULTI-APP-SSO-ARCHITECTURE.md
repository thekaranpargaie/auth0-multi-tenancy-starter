# Multi-Application Single Sign-On (SSO) Architecture

## Overview

This project implements **Cross-Application Single Sign-On (SSO)** enabling seamless authentication across multiple independent applications (CRM App, Notes App, and Main Frontend) using **Auth0** as the identity provider.

When a user logs in through one application, they can access other applications without re-entering credentials—a critical feature for enterprise SaaS platforms.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Auth0 Cloud                             │
│  (Centralized Identity Provider & Authorization Server)         │
│                                                                 │
│  - User Management                                              │
│  - Authentication                                               │
│  - Token Generation (ID Token, Access Token)                   │
│  - Role-Based Access Control                                   │
└──────────────┬──────────────────────────────────────────────────┘
               │ OIDC / OAuth2.0 Protocol
     ┌─────────┴─────────┬──────────────────┬─────────────────┐
     │                   │                  │                 │
     │                   │                  │                 │
▼────────▼───────▼────────────────▼──────────────────▼─────────────▼──
│ Frontend App  │ CRM App        │ Notes App       │ Backend API
│ (Vue 3)       │ (Vue 3)        │ (React)         │ (.NET 10)
│ :5173         │ :5174          │ :5175           │ :8000
│               │                │                 │
│ - SPA         │ - SPA          │ - SPA           │ - REST API
│ - User Pool   │ - User Pool    │ - User Pool     │ - M2M Auth
│ - Auth Store  │ - Auth Store   │ - Auth Store    │ - Token Validation
└───────────────┴────────────────┴─────────────────┴─────────────────┘
```

## Key Components

### 1. Auth0 Configuration

**Application Setup:**
- Each application (Frontend, CRM, Notes) is registered as a separate **Auth0 SPA Application**
- Each app has its own unique `Client ID` but shares the same `Auth0 Domain` and `Audience`

**Allowed Callback URLs:**
```
http://localhost:5173/callback    # Frontend
http://localhost:5174/callback    # CRM App
http://localhost:5175/callback    # Notes App
```

**Environment Variables:**
```bash
AUTH0_DOMAIN=your-tenant.auth0.com
AUTH0_AUDIENCE=https://your-api-audience
VITE_AUTH0_CLIENT_ID=frontend_client_id
VITE_CRM_CLIENT_ID=crm_client_id
VITE_NOTES_CLIENT_ID=notes_client_id
```

### 2. Frontend Application (Vue 3 - Port 5173)

**Authentication Flow:**

```
User Visits → Check Auth State → Not Authenticated → Show Login Button
                                                           ↓
                                                    User Clicks Login
                                                           ↓
                                                    Redirect to Auth0
                                                           ↓
                                                    Auth0 Login Page
                                                           ↓
                                                    User Enters Credentials
                                                           ↓
                                                    Auth0 Generates Tokens
                                                           ↓
                                                    Redirect to /callback
                                                           ↓
                                                    Store Tokens Locally
                                                           ↓
                                                    Redirect to Dashboard
```

**Technology:**
- **Auth Library:** Custom Auth store with `@auth0/auth0-spa-js`
- **State Management:** Pinia (for user state, tokens, org info)
- **Router:** Vue Router with protected routes
- **Token Storage:** Memory cache (secure, cleared on page refresh)

### 3. CRM Application (Vue 3 - Port 5174)

**Authentication Flow:**

```
User Visits → Auto-Redirect to Auth0 (No Manual Login Button)
                    ↓
            Auth0 Checks Session Cookie
                    ↓
         ┌──────────┴──────────┐
         │                      │
    User Logged In          No Session
    (Session Exists)        (First Time)
         │                      │
         ↓                      ↓
    Issue Tokens         Show Login Form
    (Silent)                    ↓
         │              User Enters Credentials
         │                    ↓
         │              Issue Tokens
         │                    ↓
         └──────────┬──────────┘
                    │
            Redirect to /callback
                    │
            handleRedirectCallback()
                    │
        Retrieve Code & State from URL
                    │
         Exchange Code for Tokens
                    │
         Store Tokens in Memory
                    │
         Redirect to /dashboard
```

**Key Features:**
- **Auto-Redirect:** When user is not authenticated, automatically triggers `authStore.login()` 
- **No Login Button UI:** Removed login button; redirect happens automatically
- **Single Init Pattern:** `authStore.init()` only called once in `main.js` to prevent state validation errors
- **Callback Protection:** Added `callbackProcessed` flag to prevent re-processing auth code

**Store Implementation (Pinia):**
```javascript
// authStore.js
const authStore = defineStore('auth', {
  state: () => ({
    client: null,
    isAuthenticated: false,
    user: null,
    orgId: null,
    orgDisplayName: null,
    permissions: [],
    accessToken: null,
    loading: true,
    error: null,
    callbackProcessed: false  // Prevent double-processing
  })
})

// In App.vue - Auto-redirect
watch(
  () => authStore.loading,
  (isLoading) => {
    if (!isLoading && !authStore.isAuthenticated) {
      authStore.login()  // Auto-redirect to Auth0
    }
  }
)
```

### 4. Notes Application (React - Port 5175)

**Authentication Flow:**

Similar to CRM App but using React patterns:

```
useEffect(() => {
  if (!isLoading && !isAuthenticated && !error) {
    loginWithRedirect()  // Auto-redirect to Auth0
  }
}, [isLoading, isAuthenticated, loginWithRedirect, error])
```

**Technology:**
- **Auth Library:** `@auth0/auth0-react` (official Auth0 React SDK)
- **State Management:** Auth0 React Hook's built-in state
- **Router:** React Router
- **Token Storage:** Memory cache (default in Auth0 React SDK)

### 5. Backend API (.NET 10 - Port 8000)

**Role:**
- **Token Validation:** Validates Auth0 access tokens from frontend requests
- **M2M Authentication:** Server-to-server authentication for management operations
- **Protected Endpoints:** Requires valid access token with audience claim

**Token Validation Flow:**
```
Frontend/App sends request with Bearer token
           ↓
Backend validates token signature & expiry
           ↓
Checks token contains correct audience
           ↓
Extracts user claims (org_id, roles, permissions)
           ↓
Authorizes request based on claims
           ↓
Returns protected resource or 401 Unauthorized
```

## SSO Mechanism: How It Works Across Apps

### Session Sharing via Auth0

**Browser Cookies:**
- Auth0 sets an **HttpOnly cookie** on the user's browser (`auth0_compat` cookie)
- This cookie persists across different subdomains/applications
- All applications on the same domain can check this cookie

**Auth Flow When User Has Active Session:**

```
User at localhost:5174/crm-app
    ↓
Click "Go to Notes App" → Navigate to localhost:5175/notes-app
    ↓
Notes App loads → Calls auth0.checkSession()
    ↓
Auth0 reads browser cookie (user already logged in)
    ↓
Silently issues new tokens for Notes App
    ↓
User is authenticated in Notes App without entering credentials
```

**Manual Token Exchange (Explicit Login):**

When user initiates login via `loginWithRedirect()`:

1. **Authorization Request:**
   ```
   GET https://tenant.auth0.com/authorize?
     client_id=CRM_CLIENT_ID
     redirect_uri=http://localhost:5174/callback
     response_type=code
     scope=openid profile email
     audience=https://api.example.com
   ```

2. **Auth0 Response (with session):**
   - Returns authorization code
   - Sets session cookie

3. **Token Exchange:**
   ```
   POST https://tenant.auth0.com/oauth/token
   {
     "client_id": "CRM_CLIENT_ID",
     "code": "authorization_code",
     "redirect_uri": "http://localhost:5174/callback",
     "grant_type": "authorization_code"
   }
   ```

4. **Tokens Returned:**
   ```json
   {
     "access_token": "eyJ0eXAiOiJKV1QiLCJhbGc...",
     "id_token": "eyJ0eXAiOiJKV1QiLCJhbGc...",
     "expires_in": 86400,
     "token_type": "Bearer"
   }
   ```

5. **Claim Examples:**
   ```json
   // ID Token Payload
   {
     "sub": "auth0|user123",
     "email": "user@example.com",
     "name": "John Doe",
     "org_id": "org_abc123",
     "org_display_name": "Acme Corp",
     "https://multi-tenant/roles": ["Member", "Admin"],
     "iat": 1234567890,
     "exp": 1234654290
   }

   // Access Token Payload
   {
     "sub": "auth0|user123",
     "aud": "https://api.example.com",
     "permissions": ["read:organizations", "write:invites"],
     "org_id": "org_abc123",
     "iat": 1234567890,
     "exp": 1234654290
   }
   ```

## Data Flow Example: Complete User Journey

### Scenario: User logs in via CRM App, then navigates to Notes App

**Step 1: User visits CRM App (localhost:5174)**
```
1. Page loads → authStore.init() runs globally
2. AuthStore checks if user is authenticated
3. User is not authenticated
4. App.vue watcher detects: !authStore.isAuthenticated && !authStore.loading
5. Triggers authStore.login() → Redirects to Auth0
```

**Step 2: Auth0 Login Page**
```
6. User sees Auth0 login form
7. Enters credentials (email/password)
8. Auth0 validates credentials
9. Auth0 issues tokens
10. Redirects to http://localhost:5174/callback?code=...&state=...
```

**Step 3: CRM App Callback Processing**
```
11. Callback.vue component loads
12. Calls authStore.init() (already running from step 1)
13. authStore detects code=... in URL
14. callbackProcessed flag prevents duplicate processing
15. Calls client.handleRedirectCallback()
16. Auth0 SDK exchanges code for tokens
17. Tokens stored in memory
18. user, orgId, orgDisplayName extracted from claims
19. URL cleaned: history.replaceState() removes ?code= and ?state=
20. Redirects to /dashboard
21. User sees dashboard content
```

**Step 4: User navigates to Notes App (localhost:5175)**
```
22. User clicks link or manually types localhost:5175
23. Notes App loads
24. useEffect checks if isAuthenticated
25. User not yet authenticated in Notes App context
26. Calls loginWithRedirect()
27. Redirects to Auth0 /authorize endpoint
28. Auth0 reads session cookie (user already logged in)
29. Silently issues new tokens for Notes App Client ID
30. Redirects to http://localhost:5175/callback
31. React App exchanges code for tokens
32. Tokens stored in Auth0 React SDK state
33. Redirects to /dashboard
34. User sees Notes App dashboard (no re-login needed!)
```

**Step 5: Backend API Call**
```
35. User action triggers API call
36. Frontend adds Authorization header:
    Header: "Authorization: Bearer <access_token>"
37. API receives request
38. Validates token signature using Auth0 public key
39. Checks exp, aud, org_id claims
40. Extracts permissions from token
41. Authorizes action based on permissions
42. Returns protected resource
```

## Token Refresh Strategy

**Initial Access Token:** Valid for 24 hours by default

**Refresh Flow (Automatic):**

```javascript
// In authStore.js
async getToken() {
  return await this.client.getTokenSilently({
    authorizationParams: { audience: auth0Config.audience }
  })
}
```

- **First call:** Returns cached token if valid
- **Token expired:** SDK automatically exchanges refresh token for new access token
- **Refresh token expired:** Triggers full re-authentication (redirect to Auth0)
- **No refresh token:** Forces re-login

## Security Considerations

### 1. **Token Storage**
- ✅ Tokens stored in **memory** (not localStorage/sessionStorage)
- ✅ Cleared automatically on page refresh
- ❌ Prevents XSS token theft from `document.cookie` access

### 2. **State Parameter Validation**
- ✅ Auth0 SDK validates state parameter in callback URL
- ✅ Prevents CSRF attacks
- ✅ CRM App added `callbackProcessed` flag to prevent replay attacks

### 3. **HTTPS in Production**
- ❌ Development: HTTP (localhost)
- ✅ Production: Must use HTTPS
- ⚠️ Auth0 domain always HTTPS (TLS 1.2+)

### 4. **CORS Configuration**
- Backend API must explicitly allow frontend origins:
  ```
  Access-Control-Allow-Origin: http://localhost:5173, http://localhost:5174, http://localhost:5175
  ```

### 5. **HttpOnly Cookies**
- Auth0 sets session cookie as HttpOnly + Secure
- JavaScript cannot access (prevents XSS exploitation)
- Automatically sent in requests (CORS: credentials included)

## File Structure

```
crm-app/
├── src/
│   ├── config/auth.js           # Auth0 config (domain, clientId, etc)
│   ├── stores/authStore.js      # Pinia store for auth state
│   ├── pages/
│   │   ├── Callback.vue         # Handles /callback route
│   │   ├── Dashboard.vue        # Protected route (requires auth)
│   │   └── Home.vue
│   ├── App.vue                  # Auto-redirect to login if not auth
│   ├── main.js                  # Initializes authStore globally
│   └── router/index.js          # Protected route guards

notes-app/
├── src/
│   ├── config/auth.js           # Auth0 config
│   ├── auth-provider.jsx        # Auth0Provider wrapper
│   ├── Callback.jsx             # Handles /callback route
│   ├── Dashboard.jsx            # Protected component
│   ├── App.jsx                  # Auto-redirect using useEffect
│   ├── main.jsx                 # Mounts React app
│   └── index.js                 # Router config

backend/
├── src/
│   ├── Auth0/                   # Auth0 management service
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Controllers/             # Protected API endpoints
└── Program.cs                   # JWT token validation middleware
```

## Testing Multi-App SSO

### Test Case 1: Single App Login
```
1. Visit http://localhost:5174
2. Automatically redirected to Auth0 login
3. Enter credentials
4. Redirected back to CRM App dashboard
✓ PASS: CRM App shows user data
```

### Test Case 2: Cross-App Session
```
1. Visit http://localhost:5174 (CRM) → Login → Dashboard
2. Visit http://localhost:5175 (Notes) in new tab
✓ PASS: Notes App auto-authenticates (no login required)
```

### Test Case 3: API Authorization
```
1. Login via CRM App
2. Click "Create Organization" button
3. Frontend sends POST /api/organizations with Bearer token
4. Backend validates token
✓ PASS: 200 OK with new org created
✗ FAIL: 401 Unauthorized (token invalid/expired)
```

### Test Case 4: Logout & Re-login
```
1. Login to CRM App
2. Click Logout button
3. auth.logout({ logoutParams: { returnTo: window.location.origin } })
4. Redirected to Auth0 logout endpoint
5. Auth0 clears session & redirects back to home
6. Try navigating to localhost:5175 (Notes)
✓ PASS: Redirected to Auth0 login again
```

### Test Case 5: Token Expiry
```
1. Login and stay idle for 25+ hours (or mock expiry)
2. Make API request
3. Backend returns 401 Unauthorized
4. Frontend SDK detects expired token
5. Calls getTokenSilently() → triggers silent refresh
6. New access token issued
7. API request retried automatically
✓ PASS: Request succeeds after refresh
```

## Troubleshooting

### "Invalid state" Error
- **Cause:** Double initialization of auth store
- **Solution:** Ensure `authStore.init()` only called once in `main.js`, not in components
- **Already Fixed in:** CRM App uses `callbackProcessed` flag

### "Unauthorized" on API Calls
- **Cause:** Missing/expired access token
- **Solution:** 
  - Verify Auth0 `--audience` matches backend audience config
  - Check token expiry: `jwt.io` decode the token
  - Ensure backend validates token signature with Auth0 public keys

### User Not Auto-Redirecting to Login
- **Cause:** Auth store loading state not properly watched
- **Solution:**
  - CRM App: Check `watch(authStore.loading, ...)`
  - Notes App: Check `useEffect([isLoading, ...], ...)`
  - Ensure conditions are correct: `!isLoading && !isAuthenticated`

### Session Not Sharing Across Apps
- **Cause:** Different domain or cookie settings
- **Solution:**
  - All apps must be on same domain (localhost for dev)
  - Auth0 session cookie must be enabled in tenant settings
  - Check browser cookie storage: DevTools → Application → Cookies

## Related Documentation

- [AUTH0-CONFIGURATION.md](AUTH0-CONFIGURATION.md) - Detailed Auth0 setup steps
- [PERMISSIONS-ROLES.md](PERMISSIONS-ROLES.md) - Role-based authorization
- [SECURITY.md](SECURITY.md) - Security best practices
- [ENVIRONMENT-VARIABLES.md](ENVIRONMENT-VARIABLES.md) - All env vars explained

## References

- [Auth0 SPA SDK Documentation](https://auth0.com/docs/libraries/auth0-spa-js)
- [Auth0 React SDK Documentation](https://auth0.com/docs/libraries/auth0-react)
- [OAuth 2.0 Authorization Code Flow](https://tools.ietf.org/html/rfc6749#section-1.3.1)
- [OpenID Connect Specification](https://openid.net/specs/openid-connect-core-1_0.html)
