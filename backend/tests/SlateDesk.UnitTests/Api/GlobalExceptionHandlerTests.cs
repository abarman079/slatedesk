using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SlateDesk.Api.Errors;

namespace SlateDesk.UnitTests.Api;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task ConcurrencyException_Returns409ProblemDetails()
    {
        var problemDetailsService =
            new CapturingProblemDetailsService();

        var handler =
            new GlobalExceptionHandler(
                NullLogger<
                    GlobalExceptionHandler>.Instance,
                problemDetailsService);

        var httpContext =
            new DefaultHttpContext();

        bool handled =
            await handler.TryHandleAsync(
                httpContext,
                new DbUpdateConcurrencyException(),
                CancellationToken.None);

        Assert.True(handled);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            httpContext.Response.StatusCode);

        Assert.NotNull(
            problemDetailsService.Captured);

        Assert.Equal(
            "Update conflict",
            problemDetailsService
                .Captured!.Title);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            problemDetailsService
                .Captured.Status);
    }

    private sealed class
        CapturingProblemDetailsService
        : IProblemDetailsService
    {
        public ProblemDetails? Captured
        {
            get;
            private set;
        }

        public ValueTask WriteAsync(
            ProblemDetailsContext context)
        {
            Captured =
                context.ProblemDetails;

            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(
            ProblemDetailsContext context)
        {
            Captured =
                context.ProblemDetails;

            return ValueTask.FromResult(true);
        }
    }
}
