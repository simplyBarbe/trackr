using backend.Common.Http;
using backend.Features.RecurringTransactions;

namespace backend.Features.RecurringTransactions.Create;

public sealed class CreateRecurringTransactionEndpoint(CreateRecurringTransactionHandler handler)
    : TrackrEndpoint<CreateRecurringTransactionRequest, RecurringTransactionResponse>
{
    public override void Configure()
    {
        Post("/api/recurring-transactions");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions"));
    }

    public override async Task HandleAsync(CreateRecurringTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        var location = result.IsSuccess ? $"/api/recurring-transactions/{result.Value!.Id}" : string.Empty;
        await SendCreatedAsync(result, location, ct);
    }
}
