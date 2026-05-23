using backend.Common.Results;
using backend.Data;
using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Rules;

public static class TransactionRules
{
    public static async Task<Result> ValidateForCreateOrUpdateAsync(
        AppDbContext db,
        TransactionType type,
        Guid accountId,
        Guid? toAccountId,
        Guid? categoryId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure(Error.Validation("Amount must be greater than zero."));

        switch (type)
        {
            case TransactionType.Income:
                if (toAccountId is not null)
                    return Result.Failure(Error.Validation("ToAccountId must be null for income transactions."));
                break;

            case TransactionType.Expense:
                if (toAccountId is not null)
                    return Result.Failure(Error.Validation("ToAccountId must be null for expense transactions."));
                break;

            case TransactionType.Transfer:
                if (toAccountId is null)
                    return Result.Failure(Error.Validation("ToAccountId is required for transfer transactions."));
                if (toAccountId == accountId)
                    return Result.Failure(Error.Validation("Transfer source and destination accounts must differ."));
                if (categoryId is not null)
                    return Result.Failure(Error.Validation("CategoryId must be null for transfer transactions."));
                break;

            default:
                return Result.Failure(Error.Validation("Invalid transaction type."));
        }

        var account = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account is null)
            return Result.Failure(Error.NotFound("Account not found."));

        if (account.IsArchived)
            return Result.Failure(Error.Validation("Cannot use an archived account."));

        if (type == TransactionType.Transfer)
        {
            var toAccount = await db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == toAccountId, cancellationToken);

            if (toAccount is null)
                return Result.Failure(Error.NotFound("Destination account not found."));

            if (toAccount.IsArchived)
                return Result.Failure(Error.Validation("Cannot transfer to an archived account."));
        }

        if (categoryId is null)
            return Result.Success();

        var category = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("Category not found."));

        if (category.IsArchived)
            return Result.Failure(Error.Validation("Cannot use an archived category."));

        var expectedKind = type switch
        {
            TransactionType.Income => CategoryKind.Income,
            TransactionType.Expense => CategoryKind.Expense,
            _ => (CategoryKind?)null
        };

        if (expectedKind is not null && category.Kind != expectedKind)
            return Result.Failure(Error.Validation($"Category kind must be {expectedKind} for {type} transactions."));

        return Result.Success();
    }

    public static ExpensePriority? ResolvePriority(
        TransactionType type,
        ExpensePriority? requestPriority,
        Category? category) =>
        type switch
        {
            TransactionType.Expense => requestPriority ?? category?.Priority ?? ExpensePriority.Discretionary,
            _ => null
        };
}
