using backend.Data.Entities.Enums;

namespace backend.Features.RecurringTransactions.Shared;

public abstract class RecurringTransactionTemplateRequest
{
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
}
