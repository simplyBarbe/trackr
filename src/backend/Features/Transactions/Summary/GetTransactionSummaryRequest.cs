using backend.Data.Entities.Enums;

namespace backend.Features.Transactions.Summary;

public sealed class GetTransactionSummaryRequest
{
    public Guid? AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public TransactionType? Type { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
