using backend.Common.Http;
using backend.Features.Transactions;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsEndpoint(ListTransactionsHandler handler)
    : TrackrEndpoint<ListTransactionsRequest, ListTransactionsResponse>
{
    public override void Configure()
    {
        Get("/api/transactions");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(ListTransactionsRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
