using backend.Application.Rules;
using backend.Common.Results;
using backend.Data;
using backend.Data.Entities;
using backend.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Transaction = backend.Data.Entities.Transaction;

namespace backend.Features.Transactions.Create;

public sealed class CreateTransactionHandler(AppDbContext db)
{
    public async Task<Result<TransactionResponse>> HandleAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
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

        Category? category = null;
        if (request.CategoryId is not null)
        {
            category = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            AccountId = request.AccountId,
            ToAccountId = request.ToAccountId,
            CategoryId = request.CategoryId,
            Priority = TransactionRules.ResolvePriority(request.Type, request.Priority, category),
            Amount = request.Amount,
            OccurredOn = request.OccurredOn,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(transaction).Reference(t => t.Account).LoadAsync(cancellationToken);
        if (transaction.ToAccountId is not null)
            await db.Entry(transaction).Reference(t => t.ToAccount).LoadAsync(cancellationToken);
        if (transaction.CategoryId is not null)
            await db.Entry(transaction).Reference(t => t.Category).LoadAsync(cancellationToken);

        return Result<TransactionResponse>.Success(TransactionMapping.ToResponse(transaction));
    }
}
