using backend.Common.Results;
using backend.Data;
using backend.Data.Entities.Enums;
using backend.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.Summary;

public sealed class GetTransactionSummaryHandler(AppDbContext db)
{
    public async Task<Result<GetTransactionSummaryResponse>> HandleAsync(
        GetTransactionSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var filtered = db.Transactions
            .AsNoTracking()
            .ApplyListFilters(
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.From,
                request.To);

        var totals = await filtered
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetTransactionSummaryResponse>.Success(
            new GetTransactionSummaryResponse(
                totals?.TotalIncome ?? 0m,
                totals?.TotalExpense ?? 0m));
    }
}
