using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.List;

public sealed class ListRecurringTransactionsHandler(AppDbContext db)
{
    public async Task<Result<ListRecurringTransactionsResponse>> HandleAsync(
        ListRecurringTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.RecurringTransactions
            .AsNoTracking()
            .Include(r => r.Account)
            .Include(r => r.ToAccount)
            .Include(r => r.Category)
            .AsQueryable();

        if (request.IsActive is not null)
            query = query.Where(r => r.IsActive == request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var rules = await query
            .OrderBy(r => r.NextOccurrenceOn)
            .ThenBy(r => r.Description)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = rules.Select(RecurringTransactionMapping.ToResponse).ToList();

        return Result<ListRecurringTransactionsResponse>.Success(
            new ListRecurringTransactionsResponse(items, request.Page, request.PageSize, totalCount));
    }
}
