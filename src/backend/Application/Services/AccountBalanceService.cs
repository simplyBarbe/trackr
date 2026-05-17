using backend.Data;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public sealed class AccountBalanceService(AppDbContext db) : IAccountBalanceService
{
    public async Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var balances = await GetBalancesAsync([accountId], cancellationToken);
        return balances.GetValueOrDefault(accountId, 0m);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetBalancesAsync(
        IEnumerable<Guid> accountIds,
        CancellationToken cancellationToken = default)
    {
        var ids = accountIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, decimal>();

        var initialBalances = await db.Accounts
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .Select(a => new { a.Id, a.InitialBalance })
            .ToDictionaryAsync(a => a.Id, a => a.InitialBalance, cancellationToken);

        var income = await SumByAccountAsync(ids, TransactionType.Income, cancellationToken);

        var expense = await SumByAccountAsync(ids, TransactionType.Expense, cancellationToken);

        var transferOut = await SumByAccountAsync(ids, TransactionType.Transfer, cancellationToken);

        var transferIn = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Type == TransactionType.Transfer && t.ToAccountId != null && ids.Contains(t.ToAccountId.Value))
            .GroupBy(t => t.ToAccountId!.Value)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Total, cancellationToken);

        var result = new Dictionary<Guid, decimal>();
        foreach (var id in ids)
        {
            if (!initialBalances.TryGetValue(id, out var balance))
                continue;

            balance += income.GetValueOrDefault(id, 0m);
            balance -= expense.GetValueOrDefault(id, 0m);
            balance -= transferOut.GetValueOrDefault(id, 0m);
            balance += transferIn.GetValueOrDefault(id, 0m);
            result[id] = balance;
        }

        return result;
    }

    private async Task<Dictionary<Guid, decimal>> SumByAccountAsync(
        List<Guid> accountIds,
        TransactionType type,
        CancellationToken cancellationToken)
    {
        return await db.Transactions
            .AsNoTracking()
            .Where(t => t.Type == type && accountIds.Contains(t.AccountId))
            .GroupBy(t => t.AccountId)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Total, cancellationToken);
    }
}
