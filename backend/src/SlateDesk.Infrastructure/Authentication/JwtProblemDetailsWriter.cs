using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SlateDesk.Infrastructure.Authentication;

internal static class JwtProblemDetailsWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string type,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType =
            "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ??
            context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            context.RequestAborted);
    }
}