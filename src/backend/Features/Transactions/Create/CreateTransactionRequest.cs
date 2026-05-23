using backend.Data.Entities.Enums;

namespace backend.Features.Transactions.Create;

public sealed class CreateTransactionRequest
{
    public TransactionType Type { get; set; }

    public Guid AccountId { get; set; }

    public Guid? ToAccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public ExpensePriority? Priority { get; set; }

    public decimal Amount { get; set; }

    public DateOnly OccurredOn { get; set; }

    public string? Description { get; set; }
}
