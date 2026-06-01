using backend.Common.Http;

namespace backend.Features.Categories.Update;

public sealed class UpdateCategoryEndpoint(UpdateCategoryHandler handler)
    : TrackrEndpoint<UpdateCategoryRequest, CategoryResponse>
{
    public override void Configure()
    {
        Put("/api/categories/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Categories"));
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
