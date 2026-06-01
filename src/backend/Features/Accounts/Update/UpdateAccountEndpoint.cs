using backend.Common.Http;

namespace backend.Features.Accounts.Update;

public sealed class UpdateAccountEndpoint(UpdateAccountHandler handler)
    : TrackrEndpoint<UpdateAccountRequest, AccountResponse>
{
    public override void Configure()
    {
        Put("/api/accounts/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Accounts"));
    }

    public override async Task HandleAsync(UpdateAccountRequest req, CancellationToken ct)
    {
        var result = await handler.HandleAsync(req, ct);
        await SendResultAsync(result, ct);
    }
}
