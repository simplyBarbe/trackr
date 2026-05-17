namespace backend.Application.Services;

public interface IAccountBalanceService
{
    Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, decimal>> GetBalancesAsync(
        IEnumerable<Guid> accountIds,
        CancellationToken cancellationToken = default);
}
