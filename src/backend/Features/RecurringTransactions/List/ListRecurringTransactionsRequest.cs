using backend.Common.Pagination;

namespace backend.Features.RecurringTransactions.List;

public sealed class ListRecurringTransactionsRequest : PagedRequest
{
    public bool? IsActive { get; set; }
}
