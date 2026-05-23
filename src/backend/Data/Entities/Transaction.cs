using backend.Data.Entities.Enums;

namespace backend.Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public TransactionType Type { get; set; }

    public Guid AccountId { get; set; }

    public Guid? ToAccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public ExpensePriority? Priority { get; set; }

    public decimal Amount { get; set; }

    public DateOnly OccurredOn { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Account Account { get; set; } = null!;

    public Account? ToAccount { get; set; }

    public Category? Category { get; set; }
}
