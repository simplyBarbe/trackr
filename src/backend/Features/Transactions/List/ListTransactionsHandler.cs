using backend.Common.Results;
using backend.Data;
using backend.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsHandler(AppDbContext db)
{
    public async Task<Result<ListTransactionsResponse>> HandleAsync(
        ListTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.ToAccount)
            .Include(t => t.Category)
            .ApplyListFilters(
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.From,
                request.To);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.OccurredOn)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = transactions.Select(TransactionMapping.ToResponse).ToList();

        return Result<ListTransactionsResponse>.Success(
            new ListTransactionsResponse(items, request.Page, request.PageSize, totalCount));
    }
}
