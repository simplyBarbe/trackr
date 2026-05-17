using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.Archive;

public sealed class ArchiveAccountHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(ArchiveAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account is null)
            return Result.Failure(Error.NotFound("Account not found."));

        if (account.IsArchived)
            return Result.Success();

        account.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
