using backend.Common.Http;
using backend.Features.Accounts;

namespace backend.Features.Accounts.Create;

public sealed class CreateAccountEndpoint(CreateAccountHandler handler)
    : TrackrEndpoint<CreateAccountRequest, AccountResponse>
{
    public override void Configure()
    {
        Post("/api/accounts");
        AllowAnonymous();
        Description(b => b.WithTags("Accounts"));
    }

    public override async Task HandleAsync(CreateAccountRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        var location = result.IsSuccess ? $"/api/accounts/{result.Value!.Id}" : string.Empty;
        await SendCreatedAsync(result, location, ct);
    }
}
