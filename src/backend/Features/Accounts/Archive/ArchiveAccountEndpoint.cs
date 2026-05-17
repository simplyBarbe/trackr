using backend.Common.Http;

namespace backend.Features.Accounts.Archive;

public sealed class ArchiveAccountEndpoint(ArchiveAccountHandler handler)
    : TrackrEndpointWithoutResponse<ArchiveAccountRequest>
{
    public override void Configure()
    {
        Post("/api/accounts/{id}/archive");
        AllowAnonymous();
        Description(b => b.WithTags("Accounts"));
    }

    public override async Task HandleAsync(ArchiveAccountRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendNoContentAsync(result, ct);
    }
}
