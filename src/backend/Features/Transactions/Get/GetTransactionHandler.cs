using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.Get;

public sealed class GetTransactionHandler(AppDbContext db)
{
    public async Task<Result<TransactionResponse>> HandleAsync(
        GetTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.ToAccount)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction is null)
            return Result<TransactionResponse>.Failure(Error.NotFound("Transaction not found."));

        return Result<TransactionResponse>.Success(TransactionMapping.ToResponse(transaction));
    }
}
