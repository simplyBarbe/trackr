using backend.Common.Results;
using backend.Data;
using backend.Data.Entities.Enums;
using backend.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.Summary;

public sealed class GetExpensesByCategoryHandler(AppDbContext db)
{
    public async Task<Result<GetExpensesByCategoryResponse>> HandleAsync(
        GetExpensesByCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var filtered = db.Transactions
            .AsNoTracking()
            .ApplyListFilters(
                null,
                null,
                TransactionType.Expense,
                null,
                request.From,
                request.To);

        var grouped = await filtered
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TotalAmount = g.Sum(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        var categoryIds = grouped
            .Where(x => x.CategoryId is not null)
            .Select(x => x.CategoryId!.Value)
            .Distinct()
            .ToList();

        var categoryNames = categoryIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Categories
                .AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var items = grouped
            .Select(x => new ExpenseByCategoryItem(
                x.CategoryId,
                x.CategoryId is null
                    ? "Uncategorized"
                    : categoryNames.GetValueOrDefault(x.CategoryId.Value) ?? "Uncategorized",
                x.TotalAmount))
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return Result<GetExpensesByCategoryResponse>.Success(new GetExpensesByCategoryResponse(items));
    }
}
