using backend.Features.RecurringTransactions.Shared;

namespace backend.Features.RecurringTransactions.Update;

public sealed class UpdateRecurringTransactionRequest : RecurringTransactionTemplateRequest
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; } = true;
}
