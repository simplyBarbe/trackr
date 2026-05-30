using backend.Data.Entities.Enums;

namespace backend.Data.Entities;

public class RecurringTransaction
{
    public Guid Id { get; set; }

    public TransactionType Type { get; set; }

    public Guid AccountId { get; set; }

    public Guid? ToAccountId { get; set; }

    public Guid? CategoryId { get; set; }

    public ExpensePriority? Priority { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public RecurrenceFrequency Frequency { get; set; }

    public DayOfWeek? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    public int? Month { get; set; }

    public DateOnly StartOn { get; set; }

    public DateOnly? EndOn { get; set; }

    public DateOnly NextOccurrenceOn { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Account Account { get; set; } = null!;

    public Account? ToAccount { get; set; }

    public Category? Category { get; set; }

    public ICollection<Transaction> GeneratedTransactions { get; set; } = [];
}
