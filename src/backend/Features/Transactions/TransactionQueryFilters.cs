using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Features.Transactions;

public static class TransactionQueryFilters
{
    public static IQueryable<Transaction> ApplyListFilters(
        this IQueryable<Transaction> query,
        Guid? accountId,
        Guid? categoryId,
        TransactionType? type,
        ExpensePriority? priority,
        DateOnly? from,
        DateOnly? to)
    {
        if (accountId is not null)
        {
            query = query.Where(t =>
                t.AccountId == accountId || t.ToAccountId == accountId);
        }

        if (categoryId is not null)
            query = query.Where(t => t.CategoryId == categoryId);

        if (type is not null)
            query = query.Where(t => t.Type == type);

        if (priority is not null)
            query = query.Where(t => t.Priority == priority);

        if (from is not null)
            query = query.Where(t => t.OccurredOn >= from);

        if (to is not null)
            query = query.Where(t => t.OccurredOn <= to);

        return query;
    }
}
