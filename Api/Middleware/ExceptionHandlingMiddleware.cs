using Microsoft.AspNetCore.Mvc;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Api.Middleware;

/// <summary>
/// Pipeline component that catches every exception escaping the controllers and the
/// application services and translates it into an RFC 7807 <c>application/problem+json</c>
/// response. Domain exceptions carry an explicit mapping (404/409/400/403); anything
/// unexpected becomes a generic 500 whose detail never leaks internals to the caller.
/// Failures are logged through <c>ILogger</c>, which Serilog captures via <c>AddSerilog</c>.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next component in the pipeline.</param>
    /// <param name="logger">The logger used to record failures.</param>
    /// <exception cref="ArgumentNullException">Thrown when an argument is <c>null</c>.</exception>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the rest of the pipeline and maps any thrown exception to a problem response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                // The body is already on the wire: the only safe option is to let the
                // host abort the connection, so the original exception is rethrown.
                _logger.LogWarning(
                    exception,
                    "Response for {Method} {Path} had already started; rethrowing.",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Writes the problem-details payload for the given exception.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The exception to translate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        (int statusCode, string title, string detail) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            // Unexpected failure: log full detail, but only expose a generic message.
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            // Known domain outcome (404/409/400/403): the message is meant for the caller.
            _logger.LogWarning(
                exception,
                "Request {Method} {Path} failed with {StatusCode}.",
                context.Request.Method,
                context.Request.Path,
                statusCode);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Correlates the response with the server-side log entries for this request.
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    /// <summary>
    /// Maps an exception to its HTTP status code, response title and client-facing detail.
    /// Derived domain exceptions are matched before their base <see cref="DomainException"/>.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The status code, title and detail for the problem response.</returns>
    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) => exception switch
    {
        EntityNotFoundException notFound =>
            (StatusCodes.Status404NotFound, "Resource not found", notFound.Message),
        DuplicateEntityException duplicate =>
            (StatusCodes.Status409Conflict, "Resource already exists", duplicate.Message),
        InsufficientStockException stock =>
            (StatusCodes.Status409Conflict, "Insufficient stock", stock.Message),
        InvalidStatusTransitionException transition =>
            (StatusCodes.Status400BadRequest, "Invalid status transition", transition.Message),
        UnauthorizedOperationException unauthorized =>
            (StatusCodes.Status403Forbidden, "Operation not allowed", unauthorized.Message),
        DomainException domain =>
            (StatusCodes.Status400BadRequest, "Business rule violated", domain.Message),
        _ =>
            (StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred while processing the request.")
    };
}
