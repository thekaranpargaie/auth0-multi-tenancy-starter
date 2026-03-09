using System.Net;
using System.Text.Json;
using Auth0MultiTenancy.Domain.Exceptions;

namespace Auth0MultiTenancy.API.Middleware;

/// <summary>
/// Global exception handler that maps domain and application exceptions
/// to appropriate HTTP responses. Keeps controllers thin and free of try/catch blocks.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            OrganizationAlreadyExistsException or UserAlreadyExistsException =>
                (HttpStatusCode.Conflict, exception.Message),

            OrganizationNotFoundException =>
                (HttpStatusCode.NotFound, exception.Message),

            UnauthorizedOrganizationAccessException or UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden, exception.Message),

            ArgumentException or InvalidOperationException =>
                (HttpStatusCode.BadRequest, exception.Message),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { detail = message }, JsonOpts);
        return context.Response.WriteAsync(body);
    }
}
