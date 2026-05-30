using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.Get;

public sealed class GetRecurringTransactionHandler(AppDbContext db)
{
    public async Task<Result<RecurringTransactionResponse>> HandleAsync(
        GetRecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await db.RecurringTransactions
            .AsNoTracking()
            .Include(r => r.Account)
            .Include(r => r.ToAccount)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
            return Result<RecurringTransactionResponse>.Failure(Error.NotFound("Recurring transaction not found."));

        return Result<RecurringTransactionResponse>.Success(RecurringTransactionMapping.ToResponse(rule));
    }
}
