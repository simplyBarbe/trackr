using backend.Common.Http;
using backend.Features.Transactions;

namespace backend.Features.Transactions.Get;

public sealed class GetTransactionEndpoint(GetTransactionHandler handler)
    : TrackrEndpoint<GetTransactionRequest, TransactionResponse>
{
    public override void Configure()
    {
        Get("/api/transactions/{id:guid}");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(GetTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
