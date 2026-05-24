using backend.Common.Http;

namespace backend.Features.Transactions.Export;

public sealed class ExportTransactionsEndpoint(ExportTransactionsHandler handler)
    : TrackrEndpoint<ExportTransactionsRequest, ExportTransactionsResponse>
{
    public override void Configure()
    {
        Get("/api/transactions/export");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(ExportTransactionsRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
