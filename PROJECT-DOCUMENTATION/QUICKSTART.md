# Quick Start Guide

## 🚀 Quick Setup in 10 Minutes

This guide will help you get the Auth0 Multi-Tenant application running quickly.

### 1. Prerequisites
- [Docker](https://docker.com) and Docker Compose installed
- [Auth0](https://auth0.com) account (free tier available)

### 2. Clone & Setup
```bash
# Clone the project
git clone <repository-url>
cd auth0-multi-tenancy-with-org

# Copy environment files
cp .env.example .env
cp backend/.env.example backend/.env
```

### 3. Auth0 Setup (5 minutes)

#### Create SPA Application
1. Go to [Auth0 Dashboard](https://manage.auth0.com) > Applications > Applications > Create Application
2. Choose **Single Page Application**
3. Name it: `Auth0 Multi-Tenant SPA`
4. Add URLs:
   - **Callback**: `http://localhost:5173/callback`
   - **Logout**: `http://localhost:5173`
   - **Web Origins**: `http://localhost:5173`
5. Note the **Client ID**

#### Create M2M Application
1. Create another application as **Machine to Machine**
2. Name it: `Auth0 Multi-Tenant Backend`
3. Select **Auth0 Management API**
4. Add permissions:
   - ✅ `create:users`
   - ✅ `read:users`
   - ✅ `create:organizations`
   - ✅ `read:organizations`
   - ✅ `create:organization_members`
   - ✅ `create:organization_invitations`
5. Note the **Client ID** and **Client Secret**

#### Get Role IDs
1. Go to **Organizations** > **Roles**
2. Note the **Admin** and **Member** role IDs (format: `rol_xxx`)

### 4. Configure Environment (3 minutes)

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
```

### 5. Start the Application (2 minutes)
```bash
# Start all services
docker-compose up -d

# Wait for services to start (30 seconds)
sleep 30

# Check health
curl http://localhost:8000/health
```

### 6. Access the Application
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:8000
- **API Documentation**: http://localhost:8000/docs

## 🎯 Test the Application

### 1. User Signup
1. Open http://localhost:5173/signup
2. Fill in:
   - Name: Your name
   - Email: Your email
   - Organization: Your company name
3. Click "Create Account"
4. Check your email for password reset link
5. Set password and log in

### 2. User Invitation
1. After login, go to Dashboard
2. Click "Manage Users"
3. Enter an email address
4. Select role (Admin/Member)
5. Click "Send Invitation"
6. Invited user receives email

## 📋 Key Concepts

### Multi-Tenant Architecture
- Each organization is isolated
- Users belong to one organization
- Role-based access control

### Authentication Flow
1. User logs in via Auth0
2. JWT tokens issued with organization claims
3. Frontend tracks organization context
4. Backend validates organization access

### Environment Variables
- **Root .env**: Frontend build-time variables
- **Backend .env**: Backend runtime variables
- **Never commit** .env files to Git

## 🔧 Common Issues

### Frontend not working
```bash
# Rebuild frontend
docker-compose build frontend
docker-compose up -d frontend
```

### Backend authentication errors
- Check M2M credentials in backend/.env
- Verify M2M application permissions

### Email not sending
- SMTP configuration is optional
- Application works without emails
- Configure SMTP in backend/.env if needed

## 📚 Next Steps

1. **Explore** the API documentation at http://8000/docs
2. **Read** the full documentation in PROJECT-DOCUMENTATION/
3. **Customize** for your use case
4. **Deploy** to production when ready

## 📞 Need Help?

- Check the [full documentation](./README.md)
- Review [API documentation](./API-DOCUMENTATION.md)
- See [Auth0 setup guide](./AUTH0-CONFIGURATION.md)

---

**Happy coding! 🎉**