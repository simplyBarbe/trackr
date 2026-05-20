using backend.Common;
using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.Shared;

public static class AccountQueryExtensions
{
    public static IQueryable<Account> Active(this IQueryable<Account> query)
        => query.Where(a => !a.IsArchived);

    public static IQueryable<Account> WithType(this IQueryable<Account> query, AccountType? type)
        => type is null ? query : query.Where(a => a.Type == type);

    public static IQueryable<Account> MatchingName(this IQueryable<Account> query, string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return query;

        var pattern = SqlLike.ContainsPattern(trimmed);
        return query.Where(a => EF.Functions.Like(a.Name, pattern, "\\"));
    }
}
