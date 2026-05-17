using backend.Common.Http;
using backend.Features.Accounts;

namespace backend.Features.Accounts.List;

public sealed class ListAccountsEndpoint(ListAccountsHandler handler)
    : TrackrEndpoint<ListAccountsRequest, ListAccountsResponse>
{
    public override void Configure()
    {
        Get("/api/accounts");
        AllowAnonymous();
        Description(b => b.WithTags("Accounts"));
    }

    public override async Task HandleAsync(ListAccountsRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
