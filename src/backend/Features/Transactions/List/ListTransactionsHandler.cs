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
            .AsQueryable();

        if (request.AccountId is not null)
        {
            query = query.Where(t =>
                t.AccountId == request.AccountId || t.ToAccountId == request.AccountId);
        }

        if (request.CategoryId is not null)
            query = query.Where(t => t.CategoryId == request.CategoryId);

        if (request.Type is not null)
            query = query.Where(t => t.Type == request.Type);

        if (request.From is not null)
            query = query.Where(t => t.OccurredOn >= request.From);

        if (request.To is not null)
            query = query.Where(t => t.OccurredOn <= request.To);

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
