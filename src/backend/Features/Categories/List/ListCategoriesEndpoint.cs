using backend.Common.Http;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesEndpoint(ListCategoriesHandler handler)
    : TrackrEndpoint<ListCategoriesRequest, ListCategoriesResponse>
{
    public override void Configure()
    {
        Get("/api/categories");
        AllowAnonymous();
        Description(b => b.WithTags("Categories"));
    }

    public override async Task HandleAsync(ListCategoriesRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
