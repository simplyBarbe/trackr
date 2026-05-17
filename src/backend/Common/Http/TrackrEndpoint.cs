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

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendNoContentAsync(Result result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.NoContentAsync(cancellation);

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendFailureAsync(Error error, CancellationToken cancellation = default) =>
        ProblemResponses.WriteAsync(HttpContext, error, cancellation);
}

public abstract class TrackrEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    protected Task SendResultAsync(Result<TResponse> result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.OkAsync(result.Value!, cancellation);

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendCreatedAsync(Result<TResponse> result, string location, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
        {
            HttpContext.Response.Headers.Location = location;
            return Send.ResponseAsync(result.Value!, StatusCodes.Status201Created, cancellation: cancellation);
        }

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendNoContentAsync(Result result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.NoContentAsync(cancellation);

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendFailureAsync(Error error, CancellationToken cancellation = default) =>
        ProblemResponses.WriteAsync(HttpContext, error, cancellation);
}

public abstract class TrackrEndpointWithoutResponse<TRequest> : Endpoint<TRequest>
    where TRequest : notnull
{
    protected Task SendNoContentAsync(Result result, CancellationToken cancellation = default)
    {
        if (result.IsSuccess)
            return Send.NoContentAsync(cancellation);

        return SendFailureAsync(result.Error!, cancellation);
    }

    protected Task SendFailureAsync(Error error, CancellationToken cancellation = default) =>
        ProblemResponses.WriteAsync(HttpContext, error, cancellation);
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
