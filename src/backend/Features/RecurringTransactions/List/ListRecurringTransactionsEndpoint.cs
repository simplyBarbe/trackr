using backend.Common.Http;

namespace backend.Features.RecurringTransactions.List;

public sealed class ListRecurringTransactionsEndpoint(ListRecurringTransactionsHandler handler)
    : TrackrEndpoint<ListRecurringTransactionsRequest, ListRecurringTransactionsResponse>
{
    public override void Configure()
    {
        Get("/api/recurring-transactions");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions"));
    }

    public override async Task HandleAsync(ListRecurringTransactionsRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
