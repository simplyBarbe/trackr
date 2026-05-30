using backend.Common.Http;
using FastEndpoints;

namespace backend.Features.RecurringTransactions.Archive;

public sealed class ArchiveRecurringTransactionEndpoint(ArchiveRecurringTransactionHandler handler)
    : TrackrEndpointWithoutResponse<ArchiveRecurringTransactionRequest>
{
    public override void Configure()
    {
        Post("/api/recurring-transactions/{id:guid}/archive");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions").Accepts<ArchiveRecurringTransactionRequest>());
    }

    public override async Task HandleAsync(ArchiveRecurringTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendNoContentAsync(result, ct);
    }
}
