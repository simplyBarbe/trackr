using backend.Common.Http;

namespace backend.Features.Categories.Create;

public sealed class CreateCategoryEndpoint(CreateCategoryHandler handler)
    : TrackrEndpoint<CreateCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Post("/api/categories");
        AllowAnonymous();
        Description(b => b.WithTags("Categories"));
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        var location = result.IsSuccess ? $"/api/categories/{result.Value!.Id}" : string.Empty;
        await SendCreatedAsync(result, location, ct);
    }
}
