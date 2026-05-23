using backend.Common.Http;

namespace backend.Features.Transactions.Delete;

public sealed class DeleteTransactionEndpoint(DeleteTransactionHandler handler)
    : TrackrEndpointWithoutResponse<DeleteTransactionRequest>
{
    public override void Configure()
    {
        Delete("/api/transactions/{id:guid}");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(DeleteTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendNoContentAsync(result, ct);
    }
}
