namespace Auth0MultiTenancy.Domain.Exceptions;

public sealed class OrganizationAlreadyExistsException(string name)
    : Exception($"Organization '{name}' already exists.");

public sealed class UserAlreadyExistsException(string email)
    : Exception($"User with email '{email}' already exists.");

public sealed class OrganizationNotFoundException(string orgId)
    : Exception($"Organization '{orgId}' was not found.");

public sealed class UnauthorizedOrganizationAccessException(string userId, string orgId)
    : Exception($"User '{userId}' is not authorized to access organization '{orgId}'.");
