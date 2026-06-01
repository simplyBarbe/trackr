using backend.Common.Pagination;
using backend.Data.Entities.Enums;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsRequest : PagedRequest
{
    public Guid? AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public TransactionType? Type { get; set; }

    public ExpensePriority? Priority { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
