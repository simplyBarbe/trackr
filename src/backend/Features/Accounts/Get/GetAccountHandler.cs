using backend.Application.Services;
using backend.Common.Results;
using backend.Features.Accounts;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.Get;

public sealed class GetAccountHandler(AppDbContext db, IAccountBalanceService balanceService)
{
    public async Task<Result<AccountResponse>> HandleAsync(
        GetAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account is null)
            return Result<AccountResponse>.Failure(Error.NotFound("Account not found."));

        var balance = await balanceService.GetBalanceAsync(account.Id, cancellationToken);
        return Result<AccountResponse>.Success(AccountMapping.ToResponse(account, balance));
    }
}
