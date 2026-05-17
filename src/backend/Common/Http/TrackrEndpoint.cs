using backend.Common.Results;
using FastEndpoints;
using MvcProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace backend.Common.Http;

public abstract class TrackrEndpointWithoutRequest<TResponse> : EndpointWithoutRequest<TResponse>
{
    protected Task SendResultAsync(Result<TResponse> result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.OkAsync(result.Value!, cancellation);

        return ProblemResponses.WriteAsync(HttpContext, result.Error!, cancellation);
    }
}

public abstract class TrackrEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    protected Task SendResultAsync(Result<TResponse> result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.OkAsync(result.Value!, cancellation);

        return ProblemResponses.WriteAsync(HttpContext, result.Error!, cancellation);
    }
}

internal static class ProblemResponses
{
    internal static async Task WriteAsync(
        HttpContext httpContext,
        Error error,
        CancellationToken cancellation)
    {
        httpContext.Response.StatusCode = error.Status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(
            new MvcProblemDetails
            {
                Status = error.Status,
                Title = error.Code,
                Detail = error.Message,
                Type = $"https://trackr.dev/errors/{error.Code}",
            },
            cancellationToken: cancellation);
    }
}
