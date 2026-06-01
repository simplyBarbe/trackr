using backend.Common.Http;

namespace backend.Features.Transactions.Summary;

public sealed class GetExpensesByCategoryEndpoint(GetExpensesByCategoryHandler handler)
    : TrackrEndpoint<GetExpensesByCategoryRequest, GetExpensesByCategoryResponse>
{
    public override void Configure()
    {
        Get("/api/transactions/summary/expenses-by-category");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(GetExpensesByCategoryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
