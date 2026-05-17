using backend.Common.Http;
using backend.Features.Transactions;

namespace backend.Features.Transactions.Create;

public sealed class CreateTransactionEndpoint(CreateTransactionHandler handler)
    : TrackrEndpoint<CreateTransactionRequest, TransactionResponse>
{
    public override void Configure()
    {
        Post("/api/transactions");
        AllowAnonymous();
        Description(b => b.WithTags("Transactions"));
    }

    public override async Task HandleAsync(CreateTransactionRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        var location = result.IsSuccess ? $"/api/transactions/{result.Value!.Id}" : string.Empty;
        await SendCreatedAsync(result, location, ct);
    }
}
