using backend.Data.Entities.Enums;

namespace backend.Features.Accounts.List;

public sealed class ListAccountsRequest
{
    public bool IncludeArchived { get; set; }

    public string? Name { get; set; }

    public AccountType? Type { get; set; }
}
