using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.Archive;

public sealed class ArchiveRecurringTransactionHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(
        ArchiveRecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
            return Result.Failure(Error.NotFound("Recurring transaction not found."));

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
