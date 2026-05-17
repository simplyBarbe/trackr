using backend.Data.Entities.Enums;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsRequest
{
    public Guid? AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public TransactionType? Type { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}
