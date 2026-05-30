using backend.Common.Http;
using FastEndpoints;

namespace backend.Features.Categories.Archive;

public sealed class ArchiveCategoryEndpoint(ArchiveCategoryHandler handler)
    : TrackrEndpointWithoutResponse<ArchiveCategoryRequest>
{
    public override void Configure()
    {
        Post("/api/categories/{id}/archive");
        AllowAnonymous();
        Description(b => b.WithTags("Categories").Accepts<ArchiveCategoryRequest>());
    }

    public override async Task HandleAsync(ArchiveCategoryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendNoContentAsync(result, ct);
    }
}
