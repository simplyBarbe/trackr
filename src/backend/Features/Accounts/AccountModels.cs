using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Features.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    decimal InitialBalance,
    decimal Balance,
    bool IsArchived,
    DateTime CreatedAt);

public sealed record ListAccountsResponse(IReadOnlyList<AccountResponse> Items);

public static class AccountMapping
{
    public static AccountResponse ToResponse(Account account, decimal balance) =>
        new(
            account.Id,
            account.Name,
            account.Type,
            account.Currency,
            account.InitialBalance,
            balance,
            account.IsArchived,
            account.CreatedAt);
}
