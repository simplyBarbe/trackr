using backend.Common.Http;

namespace backend.Features.Transactions.Summary;

public sealed class GetTransactionSummaryEndpoint(GetTransactionSummaryHandler handler)
    : TrackrEndpoint<GetTransactionSummaryRequest, GetTransactionSummaryResponse>
{
    public override void Configure()
    {
        Get("/api/transactions/summary");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(GetTransactionSummaryRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
