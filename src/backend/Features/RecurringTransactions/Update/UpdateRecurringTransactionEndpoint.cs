using backend.Common.Http;

namespace backend.Features.RecurringTransactions.Update;

public sealed class UpdateRecurringTransactionEndpoint(UpdateRecurringTransactionHandler handler)
    : TrackrEndpoint<UpdateRecurringTransactionRequest, RecurringTransactionResponse>
{
    public override void Configure()
    {
        Put("/api/recurring-transactions/{id:guid}");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions"));
    }

    public override async Task HandleAsync(UpdateRecurringTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
