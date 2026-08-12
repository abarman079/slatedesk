using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Common.Exceptions;

namespace SlateDesk.Api.Errors;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService
        _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService =
            problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemMapping mapping =
            MapException(exception);

        if (mapping.StatusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled API exception. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "API request failed with {StatusCode}. " +
                "Exception: {ExceptionType}. TraceId: {TraceId}",
                mapping.StatusCode,
                exception.GetType().Name,
                httpContext.TraceIdentifier);
        }

        var problemDetails = new ProblemDetails
        {
            Type = mapping.Type,
            Title = mapping.Title,
            Status = mapping.StatusCode,
            Detail = mapping.Detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ??
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            mapping.StatusCode;

        bool wasWritten =
            await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                });

        if (!wasWritten)
        {
            httpContext.Response.ContentType =
                "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);
        }

        return true;
    }

    private static ProblemMapping MapException(
        Exception exception)
    {
        return exception switch
        {
            TokenReplayDetectedException =>
                new ProblemMapping(
                    StatusCodes.Status401Unauthorized,
                    "https://slatedesk.local/errors/session-replay",
                    "Session invalidated",
                    "This session is no longer valid. Please sign in again."),

            AuthenticationFailedException =>
                new ProblemMapping(
                    StatusCodes.Status401Unauthorized,
                    "https://slatedesk.local/errors/authentication",
                    "Authentication failed",
                    exception.Message),

            UnauthorizedAccessException =>
                new ProblemMapping(
                    StatusCodes.Status403Forbidden,
                    "https://slatedesk.local/errors/forbidden",
                    "Access forbidden",
                    "You do not have permission to perform this action."),

            ResourceNotFoundException =>
                new ProblemMapping(
                    StatusCodes.Status404NotFound,
                    "https://slatedesk.local/errors/not-found",
                    "Resource not found",
                    exception.Message),

            ConflictException =>
                new ProblemMapping(
                    StatusCodes.Status409Conflict,
                    "https://slatedesk.local/errors/conflict",
                    "Conflict",
                    exception.Message),

            BusinessRuleException =>
                new ProblemMapping(
                    StatusCodes.Status400BadRequest,
                    "https://slatedesk.local/errors/business-rule",
                    "Business rule violated",
                    exception.Message),

            DbUpdateConcurrencyException =>
                new ProblemMapping(
                    StatusCodes.Status409Conflict,
                    "https://slatedesk.local/errors/concurrency",
                    "Update conflict",
                    "The record changed after it was loaded. Reload the latest data and try again."),

            _ =>
                new ProblemMapping(
                    StatusCodes.Status500InternalServerError,
                    "https://slatedesk.local/errors/internal",
                    "Unexpected server error",
                    "An unexpected error occurred while processing the request.")
        };
    }

    private sealed record ProblemMapping(
        int StatusCode,
        string Type,
        string Title,
        string Detail);
}