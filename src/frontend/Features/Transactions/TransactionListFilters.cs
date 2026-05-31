using Microsoft.Kiota.Abstractions;

namespace frontend.Features.Transactions;

public sealed record TransactionListFilters(
    string AccountId = "",
    string CategoryId = "",
    string Type = "",
    string Priority = "",
    Date? From = null,
    Date? To = null)
{
    public void ApplyTo(
        Action<string?> setAccountId,
        Action<string?> setCategoryId,
        Action<string?> setType,
        Action<string?> setPriority,
        Action<Date?> setFrom,
        Action<Date?> setTo)
    {
        if (!string.IsNullOrEmpty(AccountId))
            setAccountId(AccountId);

        if (!string.IsNullOrEmpty(CategoryId))
            setCategoryId(CategoryId);

        if (!string.IsNullOrEmpty(Type))
            setType(Type);

        if (!string.IsNullOrEmpty(Priority))
            setPriority(Priority);

        if (From is not null)
            setFrom(From);

        if (To is not null)
            setTo(To);
    }
}
