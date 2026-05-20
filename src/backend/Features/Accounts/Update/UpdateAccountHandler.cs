using backend.Application.Services;
using backend.Common.Results;
using backend.Features.Accounts;
using backend.Features.Accounts.Shared;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.Update;

public sealed class UpdateAccountHandler(AppDbContext db, IAccountBalanceService balanceService)
{
    public async Task<Result<AccountResponse>> HandleAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account is null)
            return Result<AccountResponse>.Failure(Error.NotFound("Account not found."));

        if (account.IsArchived)
            return Result<AccountResponse>.Failure(Error.Validation("Cannot update an archived account."));

        if (account.InitialBalance != request.InitialBalance)
        {
            var hasTransactions = await db.Transactions
                .AnyAsync(
                    t => t.AccountId == account.Id || t.ToAccountId == account.Id,
                    cancellationToken);

            if (hasTransactions)
                return Result<AccountResponse>.Failure(
                    Error.Validation("Initial balance cannot be changed when the account has transactions."));
        }

        var nameTaken = await db.Accounts
            .Active()
            .AnyAsync(
                a => a.Id != account.Id && a.Name == request.Name,
                cancellationToken);

        if (nameTaken)
            return Result<AccountResponse>.Failure(Error.Conflict("An active account with this name already exists."));

        account.Name = request.Name;
        account.Type = request.Type;
        account.Currency = request.Currency.ToUpperInvariant();
        account.InitialBalance = request.InitialBalance;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<AccountResponse>.Failure(Error.Conflict("An active account with this name already exists."));
        }

        var balance = await balanceService.GetBalanceAsync(account.Id, cancellationToken);
        return Result<AccountResponse>.Success(AccountMapping.ToResponse(account, balance));
    }
}
