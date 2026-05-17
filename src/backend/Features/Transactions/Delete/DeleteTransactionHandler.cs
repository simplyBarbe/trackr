using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.Delete;

public sealed class DeleteTransactionHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(DeleteTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction is null)
            return Result.Failure(Error.NotFound("Transaction not found."));

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
