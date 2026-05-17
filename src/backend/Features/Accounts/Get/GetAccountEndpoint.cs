using backend.Common.Http;
using backend.Features.Accounts;

namespace backend.Features.Accounts.Get;

public sealed class GetAccountEndpoint(GetAccountHandler handler)
    : TrackrEndpoint<GetAccountRequest, AccountResponse>
{
    public override void Configure()
    {
        Get("/api/accounts/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Accounts"));
    }

    public override async Task HandleAsync(GetAccountRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
