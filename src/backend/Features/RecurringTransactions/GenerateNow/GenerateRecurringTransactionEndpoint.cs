using backend.Common.Http;
using FastEndpoints;

namespace backend.Features.RecurringTransactions.GenerateNow;

public sealed class GenerateRecurringTransactionEndpoint(GenerateRecurringTransactionHandler handler)
    : TrackrEndpointWithoutRequest<GenerateRecurringTransactionResponse>
{
    public override void Configure()
    {
        Post("/api/recurring-transactions/{id:guid}/generate-now");
        AllowAnonymous();
        Description(b => b.WithTags("RecurringTransactions").Accepts<EmptyRequest>());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var request = new GenerateRecurringTransactionRequest { Id = Route<Guid>("id") };
        var result = await handler.HandleAsync(request, ct);
        await SendResultAsync(result, ct);
    }
}
