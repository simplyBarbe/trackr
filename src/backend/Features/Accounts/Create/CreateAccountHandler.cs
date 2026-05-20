using backend.Application.Services;
using backend.Common.Results;
using backend.Features.Accounts;
using backend.Features.Accounts.Shared;
using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.Create;

public sealed class CreateAccountHandler(AppDbContext db, IAccountBalanceService balanceService)
{
    public async Task<Result<AccountResponse>> HandleAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var nameExists = await db.Accounts
            .Active()
            .AnyAsync(a => a.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result<AccountResponse>.Failure(Error.Conflict("An active account with this name already exists."));

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Currency = request.Currency.ToUpperInvariant(),
            InitialBalance = request.InitialBalance,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);

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
