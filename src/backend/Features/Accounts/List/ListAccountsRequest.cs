using backend.Data.Entities.Enums;
using backend.Common.Pagination;

namespace backend.Features.Accounts.List;

public sealed class ListAccountsRequest : PagedRequest
{
    public bool? IncludeArchived { get; set; }

    public string? Name { get; set; }

    public AccountType? Type { get; set; }
}
