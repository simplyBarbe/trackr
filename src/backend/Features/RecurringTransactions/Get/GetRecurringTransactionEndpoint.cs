using backend.Common.Http;

namespace backend.Features.RecurringTransactions.Get;

public sealed class GetRecurringTransactionEndpoint(GetRecurringTransactionHandler handler)
    : TrackrEndpoint<GetRecurringTransactionRequest, RecurringTransactionResponse>
{
    public override void Configure()
    {
        Get("/api/recurring-transactions/{id:guid}");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions"));
    }

    public override async Task HandleAsync(GetRecurringTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
