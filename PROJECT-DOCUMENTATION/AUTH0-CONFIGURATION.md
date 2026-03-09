# Auth0 Configuration Requirements

## Overview

This project requires specific Auth0 configuration for multi-tenant functionality. The configuration includes two applications, organization setup, and proper permissions.

## Prerequisites

1. **Auth0 Account** with Organizations enabled
2. **Tenant Access** to configure applications and settings
3. **API Access** to Auth0 Dashboard

## Step 1: Enable Organizations

### 1.1 Enable Organization Feature
1. Go to **Auth0 Dashboard** > **Settings** > **Organization**
2. Enable Organizations if not already enabled
3. Configure organization branding (optional)
4. Save changes

### 1.2 Configure Organization Domain
1. Under **Organization** > **Settings**
2. Set your organization domain (if you have one)
3. This is optional but recommended for production

## Step 2: Create SPA Application

### 2.1 Create Application
1. Go to **Applications** > **Applications** > **Create Application**
2. Choose **Single Page Web Applications**
3. Fill in basic information:
   - **Name**: `Auth0 Multi-Tenant SPA`
   - **Description**: `Frontend application for multi-tenant SaaS`

### 2.2 Configure Settings
1. **Application URIs**:
   - **Allowed Callback URLs**: `http://localhost:5173/callback`
   - **Allowed Logout URLs**: `http://localhost:5173`
   - **Allowed Web Origins**: `http://localhost:5173`

2. **Advanced Settings** (if needed):
   - **JSON Web Token Signature Algorithm**: `RS256`
   - **Auth0 APIs**:
     - Enable **Auth0 Management API**
     - Add audience: `https://YOUR_TENANT.auth0.com/api/v2/`

### 2.3 Get Client ID
- After creating the application, note the **Client ID**
- This will be used in both frontend and backend configuration

## Step 3: Create M2M Application

### 3.1 Create Application
1. Go to **Applications** > **Applications** > **Create Application**
2. Choose **Machine to Machine Applications**
3. Fill in:
   - **Name**: `Auth0 Multi-Tenant Backend M2M`
   - **Description**: `Backend API access to Auth0`

### 3.2 Configure API Permissions
1. Select the following APIs:
   - **Auth0 Management API**
2. Grant the following permissions:
   - Users:
     - ✅ `create:users`
     - ✅ `read:users`
     - ✅ `update:users`
   - Organizations:
     - ✅ `create:organizations`
     - ✅ `read:organizations`
   - Organization Members:
     - ✅ `create:organization_members`
   - Organization Invitations:
     - ✅ `create:organization_invitations`
   - User Tickets (for password reset):
     - ✅ `create:user_tickets`

### 3.3 Get Credentials
- Note the **Client ID** and **Client Secret**
- These will be used in the backend configuration

## Step 4: Configure Organization Roles

### 4.1 Create Custom Roles
1. Go to **Organizations** > **Roles**
2. Create two roles:

#### Admin Role
- **Name**: `Admin`
- **Description**: `Full access to organization`
- **Permissions**: All organization permissions

#### Member Role
- **Name**: `Member`
- **Description**: `Basic organization member access`
- **Permissions**: Limited permissions as needed

### 4.2 Note Role IDs
- Role IDs are in format `rol_xxx` (e.g., `rol_CfCLnlOA8pCGEo3T`)
- These IDs are needed for backend configuration

### 4.3 Assign Roles to Organization
1. Go to **Organizations** > **Your Organization**
2. Under **Roles**, assign the Admin and Member roles as needed

## Step 5: Configure Connection

### 5.1 Username-Password-Authentication
1. Go to **Authentication** > **Database**
2. Ensure **Username-Password-Authentication** is enabled
3. This is the default connection for user creation

### 5.2 Optional: Configure Connection
- Enable "Requires username" if needed
- Configure password policy if needed
- Email verification as needed

## Step 6: Test Configuration

### 6.1 Test Login Flow
1. Use Auth0 Universal Login
2. Test organization creation during signup
3. Verify user can log in and access organization

### 6.2 Test API Access
1. Use the backend API endpoints
2. Test user creation
3. Test organization creation
4. Test invitation flow

## Environment Mapping

### Frontend (SPA Application)
```env
VITE_AUTH0_DOMAIN=your-tenant.auth0.com
VITE_AUTH0_CLIENT_ID=spa_client_id_here
VITE_AUTH0_AUDIENCE=https://your-tenant.auth0.com/api/v2/
```

### Backend (M2M Application)
```env
AUTH0_DOMAIN=your-tenant.auth0.com
AUTH0_M2M_CLIENT_ID=m2m_client_id_here
AUTH0_M2M_CLIENT_SECRET=m2m_client_secret_here
AUTH0_APP_CLIENT_ID=spa_client_id_here
AUTH0_AUDIENCE=https://your-tenant.auth0.com/api/v2/
AUTH0_ADMIN_ROLE_ID=rol_xxx
AUTH0_MEMBER_ROLE_ID=rol_yyy
```

## Common Issues and Solutions

### 1. "Invalid Organization" Error
- **Cause**: Organizations feature not enabled
- **Solution**: Enable Organizations in Auth0 tenant settings

### 2. "Unauthorized" on API Calls
- **Cause**: M2M application lacks permissions
- **Solution**: Verify all required permissions are granted

### 3. Role Assignment Fails
- **Cause**: Incorrect role ID
- **Solution**: Verify role IDs from Organizations > Roles

### 4. Callback URL Mismatch
- **Cause**: SPA callback URL not configured
- **Solution**: Ensure exact match in SPA application settings

### 5. JWT Verification Fails
- **Cause**: Wrong issuer or audience
- **Solution**: Verify all Auth0 domain and audience URLs

## Security Considerations

1. **Never expose M2M credentials** in frontend code
2. **Use secure Client Secrets** - store them as environment variables
3. **Implement proper CORS** in your Auth0 applications
4. **Regularly rotate credentials** for security
5. **Monitor API usage** for suspicious activity

## Production Checklist

- [ ] Organizations feature enabled
- [ ] SPA application configured with production URLs
- [ ] M2M application with all required permissions
- [ ] Custom roles created and assigned
- [ ] Connection properly configured
- [ ] All environment variables set correctly
- [ ] JWT verification working
- [ ] CORS configured for production domains
- [ ] SSL/TLS enabled for all endpoints

## Additional Resources

- [Auth0 Documentation](https://auth0.com/docs)
- [Auth0 Management API Reference](https://auth0.com/docs/api/management/v2)
- [Auth0 Organizations Documentation](https://auth0.com/docs/organizations)
- [Auth0 Machine to Machine Applications](https://auth0.com/docs/applications/machine-to-machine)