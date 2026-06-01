using backend.Common.Http;

namespace backend.Features.Categories.Get;

public sealed class GetCategoryEndpoint(GetCategoryHandler handler)
    : TrackrEndpoint<GetCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Get("/api/categories/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Categories"));
    }

    public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
