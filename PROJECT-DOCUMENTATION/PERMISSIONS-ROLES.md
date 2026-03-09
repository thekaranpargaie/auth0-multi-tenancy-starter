# Permissions and Role Management

## Overview

The project implements role-based access control (RBAC) using Auth0 Organizations. This allows for multi-tenant applications where users can belong to different organizations with varying levels of permissions.

## Permission Architecture

### 1. System-Wide Permissions (M2M Application)

These permissions are assigned to the Machine-to-Machine application and are used by the backend API to interact with Auth0 Management API.

#### Required M2M Permissions
```json
{
  "create:users": "Create new users",
  "read:users": "Read user information",
  "update:users": "Update user information",
  "create:organizations": "Create new organizations",
  "read:organizations": "Read organization information",
  "create:organization_members": "Add users to organizations",
  "create:organization_invitations": "Create organization invitations",
  "create:user_tickets": "Create password reset tickets"
}
```

#### Backend Service Permissions
The backend uses these permissions to:
- Create organizations during user signup
- Create users for signup and invitation flows
- Add users to organizations with specific roles
- Send password reset emails
- Manage organization connections

### 2. Organization Roles (Custom Roles)

These are custom roles defined within Auth0 Organizations that can be assigned to users.

#### Default Roles

##### Admin Role
- **Purpose**: Full administrative access to the organization
- **Permissions**: All organization-related permissions
- **Typical Users**: Organization founders, primary administrators

##### Member Role
- **Purpose**: Basic member access to the organization
- **Permissions**: Limited access based on application requirements
- **Typical Users**: Regular organization members

#### Role Assignment Flow
1. User signs up or is invited → User created in Auth0
2. User added to organization → Basic membership assigned
3. Role assigned → Specific permissions granted
4. Permissions enforced via JWT claims

## Permission Implementation

### 1. Backend Permission Checks

The backend verifies JWT tokens and extracts organization information:

```python
# JWT verification ensures token is valid
# Token contains:
# - org_id: Organization ID
# - org_name: Organization name
# - org_display_name: Organization display name
```

### 2. Frontend Permission Handling

Frontend uses Auth0 SPA SDK to get user and organization information:

```javascript
// Get user info with organization context
const orgInfo = await getOrganizationInfo();
if (orgInfo.organization_id) {
  // User belongs to an organization
  const permissions = orgInfo.permissions;

  if (permissions.includes('manage_users')) {
    // Show user management UI
  }
}
```

### 3. API Permission Validation

The backend validates permissions for protected endpoints:

```python
# Example: Check if user can invite others
async def can_invite_user(user_token: str, org_id: str):
    # Verify token belongs to the organization
    if not verify_org_membership(user_token, org_id):
        return False

    # Check if user has permission
    permissions = get_user_permissions(user_token)
    return 'create:organization_invitations' in permissions
```

## Role Management API

### 1. Role Assignment

#### Assign Admin Role
```python
await auth0_service.assign_organization_role(
    organization_id=org_id,
    user_id=user_id,
    role_id=settings.auth0_admin_role_id
)
```

#### Assign Member Role
```python
await auth0_service.assign_organization_role(
    organization_id=org_id,
    user_id=user_id,
    role_id=settings.auth0_member_role_id
)
```

#### Remove Role
```python
await auth0_service.remove_organization_role(
    organization_id=org_id,
    user_id=user_id,
    role_id=role_id
)
```

### 2. User Invitation with Role

When inviting users, you can specify the role:

```python
# Invite as Admin
role_id = settings.auth0_admin_role_id

# Invite as Member
role_id = settings.auth0_member_role_id

await auth0_service.add_user_to_organization(
    organization_id=org_id,
    user_id=user_id,
    roles=[role_id]
)
```

## Permission Levels

### System Level
- **M2M Application**: Has system-wide permissions to manage Auth0 resources
- **Backend API**: Acts on behalf of the system to manage users and organizations

### Organization Level
- **Admin**: Full control over organization resources
- **Member**: Limited access based on application requirements

### Application Level (Customizable)
You can extend the permission system by:

1. **Adding Custom Permissions**
   - Define new organization roles in Auth0
   - Update backend to handle new roles
   - Implement permission checks in API endpoints

2. **Implementing Application-Level RBAC**
   - Store additional permissions in user metadata
   - Create permission middleware for protected endpoints
   - Use database for application-specific permissions

## Security Best Practices

### 1. Principle of Least Privilege
- Only assign necessary permissions to users
- Regularly review role assignments
- Remove unnecessary permissions when roles change

### 2. Audit Trail
- Log all permission changes
- Track role assignments and removals
- Monitor for unusual permission usage

### 3. Regular Review
- Review organization roles quarterly
- Audit user permissions periodically
- Remove inactive users and their permissions

### 4. Separation of Duties
- Ensure critical functions require multiple approvals
- Avoid having single users with excessive permissions
- Implement approval workflows for sensitive operations

## Common Permission Scenarios

### 1. User Signup Flow
1. User signs up → Created without specific organization role
2. Organization created → Admin role automatically assigned to signup user
3. User can now manage their organization

### 2. User Invitation Flow
1. Admin invites user → Can specify Admin or Member role
2. User created → Assigned specified role
3. User gains permissions based on assigned role

### 3. Role Changes
1. Admin promotes Member to Admin
2. Backend assigns Admin role ID
3. User gains elevated permissions immediately
4. Previous role can be removed if needed

## Troubleshooting Permission Issues

### 1. User Lacking Permissions
- **Check**: Verify role assignment in Auth0
- **Check**: Confirm JWT token contains organization claims
- **Check**: Backend configuration has correct role IDs

### 2. API Permission Denied
- **Check**: M2M application has required permissions
- **Check**: JWT verification working correctly
- **Check**: Token contains proper organization context

### 3. Role Assignment Fails
- **Check**: Role IDs are correct
- **Check**: User is already in organization
- **Check**: No conflicting role assignments

## Future Enhancements

### 1. Granular Permissions
Instead of just Admin/Member, you could implement:
- `view_users`: Can view organization users
- `manage_users`: Can add/remove users
- `view_settings`: Can view organization settings
- `manage_settings`: Can modify organization settings

### 2. Permission Inheritance
Create hierarchical roles:
- `Super Admin`: Inherits all permissions
- `Admin`: Inherits most permissions
- `Manager`: Inherits limited permissions
- `Member`: Basic permissions only

### 3. Permission Analytics
- Track permission usage across the organization
- Identify permission bottlenecks
- Recommend permission optimizations