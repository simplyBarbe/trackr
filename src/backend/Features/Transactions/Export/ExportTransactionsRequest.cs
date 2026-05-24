using backend.Data.Entities.Enums;

namespace backend.Features.Transactions.Export;

public sealed class ExportTransactionsRequest
{
    public Guid? AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public TransactionType? Type { get; set; }

    public ExpensePriority? Priority { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
