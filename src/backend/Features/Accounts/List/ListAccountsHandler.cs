using backend.Application.Services;
using backend.Common.Results;
using backend.Features.Accounts;
using backend.Features.Accounts.Shared;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Accounts.List;

public sealed class ListAccountsHandler(AppDbContext db, IAccountBalanceService balanceService)
{
    public async Task<Result<ListAccountsResponse>> HandleAsync(
        ListAccountsRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Accounts.AsNoTracking();

        if (!request.IncludeArchived)
            query = query.Active();

        query = query
            .WithType(request.Type)
            .MatchingName(request.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .OrderBy(a => a.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var balances = await balanceService.GetBalancesAsync(
            accounts.Select(a => a.Id),
            cancellationToken);

        var items = accounts
            .Select(a => AccountMapping.ToResponse(a, balances.GetValueOrDefault(a.Id, a.InitialBalance)))
            .ToList();

        return Result<ListAccountsResponse>.Success(
            new ListAccountsResponse(items, request.Page, request.PageSize, totalCount));
    }
}
