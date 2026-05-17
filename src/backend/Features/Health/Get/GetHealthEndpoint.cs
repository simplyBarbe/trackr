using backend.Common.Http;

namespace backend.Features.Health.Get;

public sealed class GetHealthEndpoint(GetHealthHandler handler) : TrackrEndpointWithoutRequest<GetHealthResponse>
{
    public override void Configure()
    {
        Get("/api/health");
        AllowAnonymous();
        Description(b => b.WithTags("Health"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await handler.HandleAsync(ct);
        await SendResultAsync(result, ct);
    }
}
