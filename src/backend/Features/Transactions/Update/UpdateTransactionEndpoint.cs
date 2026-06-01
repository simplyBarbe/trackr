using backend.Common.Http;

namespace backend.Features.Transactions.Update;

public sealed class UpdateTransactionEndpoint(UpdateTransactionHandler handler)
    : TrackrEndpoint<UpdateTransactionRequest, TransactionResponse>
{
    public override void Configure()
    {
        Put("/api/transactions/{id:guid}");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(UpdateTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
