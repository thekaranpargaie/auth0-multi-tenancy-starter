# Workflows Documentation

## Overview

This document details the key workflows in the Auth0 Multi-Tenant application. Each workflow includes the user journey, system interactions, and technical implementation details.

## 1. User Signup Workflow

### Overview
Complete process from user registration to active organization membership.

### Flow Diagram
```
User visits signup → Form submission → Backend validates →
Creates organization → Creates user → Adds to organization →
Sends email → Sets password → User logs in → Dashboard access
```

### Step-by-Step Process

#### Step 1: User Accesses Signup Page
- **URL**: `/signup`
- **Action**: User fills signup form
- **Required Fields**:
  - Name
  - Email
  - Organization Name
  - Organization Domain (optional)

#### Step 2: Form Submission
- **Endpoint**: `POST /api/signup`
- **Request Body**:
  ```json
  {
    "name": "John Doe",
    "email": "john@example.com",
    "organization_name": "Acme Corp",
    "organization_domain": "acme.com"
  }
  ```

#### Step 3: Backend Processing
1. **Validate Input**:
   - Check email format
   - Sanitize organization name
   - Validate organization domain format

2. **Generate Organization ID**:
   ```python
   # Use email domain or sanitized name
   org_name = request.organization_domain.split('.')[0] or sanitize_name(request.organization_name)
   ```

3. **Create Organization**:
   ```python
   org = await auth0_service.create_organization(
       name=org_name,
       display_name=request.organization_name,
       domain=request.organization_domain,
       metadata={"created_by": "signup_api"}
   )
   ```

4. **Create User**:
   ```python
   user = await auth0_service.create_user(
       email=request.email,
       name=request.name,
       password=temporary_password,
       connection="Username-Password-Authentication",
       email_verified=True
   )
   ```

5. **Add User to Organization**:
   ```python
   await auth0_service.add_user_to_organization(
       organization_id=org["id"],
       user_id=user["user_id"],
       roles=[admin_role_id]
   )
   ```

6. **Send Email**:
   - Password reset ticket
   - Welcome email (if SMTP configured)

#### Step 4: User Email Workflow
1. **User receives email** with password reset link
2. **Clicks link** to Auth0 password reset page
3. **Sets new password**
4. **Returns to application**

#### Step 5: User Login
1. **Auth0 authentication** completes
2. **JWT tokens issued** with organization claims
3. **User redirected** to dashboard
4. **Frontend detects organization membership**
5. **Dashboard loads** with organization context

### Error Handling
- **Organization exists**: Return error if organization name conflicts
- **Email exists**: Return error if email already registered
- **Auth0 API errors**: Handle with retry logic and user-friendly messages
- **Email failures**: Log error but continue with signup

### Success Metrics
- Signup completion rate
- Email open rate
- Password reset completion rate

## 2. User Invitation Workflow

### Overview
Process for existing organization members to invite new users.

### Flow Diagram
```
Admin visits dashboard → Clicks "Invite Users" →
Enters email and role → Backend processes →
Creates user → Adds to organization →
Sends invitation email → New user joins
```

### Step-by-Step Process

#### Step 1: Initiate Invitation
- **Location**: Dashboard > User Management
- **Action**: Click "Invite User"
- **Form Fields**:
  - Email address
  - Role (Admin/Member - default: Member)

#### Step 2: Submit Invitation
- **Endpoint**: `POST /api/invite-user`
- **Authentication**: Required (JWT with organization context)
- **Request Body**:
  ```json
  {
    "organization_id": "org_123",
    "email": "jane@example.com",
    "role": "member"
  }
  ```

#### Step 3: Backend Processing
1. **Validate Request**:
   - Check JWT contains organization ID
   - Verify user has permission to invite
   - Validate email format

2. **Create User**:
   ```python
   user = await auth0_service.create_user(
       email=request.email,
       name=request.email.split('@')[0],
       password=temporary_password,
       connection="Username-Password-Authentication",
       email_verified=True
   )
   ```

3. **Add to Organization**:
   ```python
   role_id = admin_role_id if request.role == "admin" else member_role_id
   await auth0_service.add_user_to_organization(
       organization_id=request.organization_id,
       user_id=user["user_id"],
       roles=[role_id]
   )
   ```

4. **Send Email**:
   - Password reset ticket
   - Welcome email with organization details

#### Step 4: New User Onboarding
1. **Receives invitation email**
2. **Clicks password reset link**
3. **Sets password**
4. **Logs into application**
5. **Has access to organization**

### Permission Checks
- Only organization members can invite others
- Admins can invite as Admin or Member
- Members can only invite as Member

### Error Handling
- **Invalid organization**: 403 Forbidden
- **Insufficient permissions**: 403 Forbidden
- **Email already exists**: 409 Conflict
- **Rate limiting**: 429 Too Many Requests

## 3. Authentication Workflow

### Overview
Process for user authentication and session management.

### Login Flow
```
User clicks login → Auth0 Universal Login →
User credentials verified → JWT issued →
Redirect to callback → Tokens stored →
Organization context loaded → User authenticated
```

### Step-by-Step Process

#### Step 1: Login Initiation
- **Action**: User clicks "Log In with Auth0"
- **Frontend Call**:
  ```javascript
  await login({
    organization: userContext.organization // Optional
  });
  ```

#### Step 2: Auth0 Universal Login
- **User Experience**: Auth0-hosted login page
- **Features**:
  - Social login (if configured)
  - Organization selection (if enabled)
  - Password reset option
  - MFA (if configured)

#### Step 3: Callback Processing
- **URL**: `/callback`
- **Action**: Handles Auth0 redirect
- **Processing**:
  ```javascript
  const result = await handleRedirectCallback();
  // Extract tokens and user info
  ```

#### Step 4: Session Management
- **Token Storage**:
  - Access token in memory (for API calls)
  - ID token claims in memory
  - Refresh tokens for long sessions

#### Step 5: Organization Context
- **Frontend Loads**:
  ```javascript
  const orgInfo = await getOrganizationInfo();
  // Set organization in app state
  ```

### Logout Flow
```
User clicks logout → Clear local tokens →
Auth0 logout → Redirect to homepage →
Session fully terminated
```

### Session Security
- JWT tokens signed with RS256
- Access tokens short-lived (15-60 min)
- Refresh tokens for session continuity
- Token validation on every API call

## 4. Organization Management Workflow

### Overview
Process for managing organization settings and members.

### Key Operations

#### 1. View Organization Details
- **Endpoint**: Backend queries Auth0 API
- **Data Retrieved**:
  - Organization name and display name
  - Member count
  - Created date
  - Domain (if set)

#### 2. Manage Users
- **List Members**: Query organization members
- **Change Roles**: Assign/remove role IDs
- **Remove Members**: Delete from organization

#### 3. Update Organization Settings
- **Display Name**: Update via Auth0 API
- **Domain**: Update via Auth0 API
- **Metadata**: Custom organization data

### Permission Matrix

| Operation | Admin | Member |
|-----------|-------|--------|
| View Org Info | ✅ | ✅ |
| List Users | ✅ | ✅ |
| Invite Users | ✅ | ❌ |
| Change Roles | ✅ | ❌ |
| Remove Users | ✅ | ❌ |
| Update Settings | ✅ | ❌ |

## 5. Development Workflows

### Local Development
```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f backend
docker-compose logs -f frontend

# Stop services
docker-compose down

# Rebuild specific service
docker-compose build frontend
```

### Code Changes
1. **Backend Changes**:
   - Auto-reloads with `uvicorn --reload`
   - Changes take effect immediately

2. **Frontend Changes**:
   - Hot reload during development
   - Build required for production changes

### Testing Workflow
1. **Unit Tests**: pytest for backend services
2. **Integration Tests**: Test API endpoints
3. **E2E Tests**: Test user flows with Playwright
4. **Auth0 Tests**: Test Auth0 integration

## 6. Deployment Workflow

### Production Deployment
```bash
# Build and deploy
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Check health
curl http://your-domain.com/health

# View logs
docker-compose logs -f --tail=100
```

### Environment Management
- **Development**: Local Docker Compose
- **Staging**: Pre-production environment
- **Production**: Live environment with SSL

### Monitoring
- **Health Checks**: Endpoint monitoring
- **Logging**: Structured logs with timestamps
- **Error Tracking**: Integration with error tracking service
- **Performance**: API response time monitoring

## 7. Error Recovery Workflows

### Auth0 API Failures
1. **Retry Logic**: Exponential backoff
2. **Fallback**: Use cached data where possible
3. **User Notification**: Clear error messages
4. **Alerting**: Notify dev team for critical failures

### Database Issues
1. **Connection Retry**: Auto-reconnect
2. **Read Replicas**: For read-heavy operations
3. **Cache Fallback**: Use Redis cache
4. **Graceful Degradation**: Limited functionality

### Email Delivery Failures
1. **Retry Queue**: Queue failed emails
2. **Fallback SMTP**: Secondary email provider
3. **User Notification**: Inform of issue
4. **Monitoring**: Track delivery rates

## 8. Performance Optimization Workflows

### Caching Strategy
1. **JWT Caching**: Cache verified tokens
2. **Organization Data**: Cache member lists
3. **API Response**: Cache frequent queries
4. **Static Assets**: CDN for frontend

### Database Optimization
1. **Indexing**: Proper indexing on frequently queried fields
2. **Connection Pooling**: Reuse database connections
3. **Query Optimization**: Avoid N+1 queries
4. **Replication**: Read replicas for scalability

### Auth0 Optimization
1. **Token Caching**: Cache Auth0 JWKS
2. **Batch Operations**: Group multiple API calls
3. **Connection Pooling**: Reuse HTTP clients
4. **Rate Limiting**: Respect Auth0 rate limits

## 9. Security Workflows

### Token Validation
1. **JWT Verification**: Validate signature and claims
2. **Blacklist**: Check token against revocation list
3. **Expiration**: Validate token expiry
4. **Audience**: Validate token audience

### Access Control
1. **Organization Context**: Ensure user belongs to requested org
2. **Permission Checks**: Validate specific permissions
3. **Rate Limiting**: Prevent abuse
4. **Input Validation**: Sanitize all inputs

### Audit Logging
1. **Authentication Events**: Log all login attempts
2. **API Calls**: Log all endpoint access
3. **Permission Changes**: Log role assignments
4. **Error Events**: Log security-related errors