using backend.Application.Rules;
using backend.Common.Results;
using backend.Data;
using backend.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Transactions.Update;

public sealed class UpdateTransactionHandler(AppDbContext db)
{
    public async Task<Result<TransactionResponse>> HandleAsync(
        UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions
            .Include(t => t.Account)
            .Include(t => t.ToAccount)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction is null)
            return Result<TransactionResponse>.Failure(Error.NotFound("Transaction not found."));

        var validation = await TransactionRules.ValidateForCreateOrUpdateAsync(
            db,
            request.Type,
            request.AccountId,
            request.ToAccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (!validation.IsSuccess)
            return Result<TransactionResponse>.Failure(validation.Error!);

        transaction.Type = request.Type;
        transaction.AccountId = request.AccountId;
        transaction.ToAccountId = request.ToAccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.Amount = request.Amount;
        transaction.OccurredOn = request.OccurredOn;
        transaction.Description = request.Description;

        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(transaction).Reference(t => t.Account).LoadAsync(cancellationToken);
        if (transaction.ToAccountId is not null)
            await db.Entry(transaction).Reference(t => t.ToAccount).LoadAsync(cancellationToken);
        else
            transaction.ToAccount = null;

        if (transaction.CategoryId is not null)
            await db.Entry(transaction).Reference(t => t.Category).LoadAsync(cancellationToken);
        else
            transaction.Category = null;

        return Result<TransactionResponse>.Success(TransactionMapping.ToResponse(transaction));
    }
}
